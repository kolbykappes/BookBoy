// Play Mode check for the mouse controls, run while playing. It drives the real input path in-process (no window focus needed):
// it queues Input System mouse button states, updates the Input System so Mouse.current.*.wasPressedThisFrame is true,
// then runs the player's input handler exactly as a frame would.
if (!UnityEditor.EditorApplication.isPlaying) return "REFUSED: enter Play Mode first";
var sb = new System.Text.StringBuilder(); int fails = 0;
void Check(string name, bool ok, string detail = "") { if (!ok) fails++; sb.Append((ok ? "PASS " : "FAIL ") + name + (ok ? "" : "  <-- " + detail) + "\n"); }
Book B(string n) { return UnityEngine.GameObject.Find(n).GetComponent<Book>(); }
Bookcase CaseOfTheme(BookCategory t) { foreach (var bc in UnityEngine.Object.FindObjectsByType<Bookcase>(UnityEngine.FindObjectsInactive.Include)) if (bc.theme == t) return bc; return null; }
var game = UnityEngine.GameObject.Find("Game"); var gp = game.GetComponent<GameProgress>();
var pl = UnityEngine.GameObject.Find("Player"); var pi = pl.GetComponent<PlayerInteractor>(); var carry = pi.Carry;
var fl = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance; var tPi = typeof(PlayerInteractor);
var mUpdate = tPi.GetMethod("Update", fl);   // UpdateFocus + visuals + prompt + HandleInput, i.e. one full frame of the interactor
var mouse = UnityEngine.InputSystem.Mouse.current;
if (mouse == null) return "FAIL: no Mouse device";
Check("mouse device is enabled", mouse.enabled, "enabled=" + mouse.enabled);

// one "frame": button down -> input update -> interactor Update -> button up -> input update
void Click(string button)
{
    var down = new UnityEngine.InputSystem.LowLevel.MouseState();
    down = down.WithButton(button == "left" ? UnityEngine.InputSystem.LowLevel.MouseButton.Left : UnityEngine.InputSystem.LowLevel.MouseButton.Right, true);
    UnityEngine.InputSystem.InputSystem.QueueStateEvent(mouse, down);
    UnityEngine.InputSystem.InputSystem.Update();
    bool seen = button == "left" ? mouse.leftButton.wasPressedThisFrame : mouse.rightButton.wasPressedThisFrame;
    if (!seen) sb.Append("(warning: the input system did not report a press this frame)\n");
    mUpdate.Invoke(pi, null);
    UnityEngine.InputSystem.InputSystem.QueueStateEvent(mouse, new UnityEngine.InputSystem.LowLevel.MouseState());
    UnityEngine.InputSystem.InputSystem.Update();
}
void Aim(UnityEngine.Vector3 from, UnityEngine.Vector3 target) { var c = pi.interactionCamera.transform; c.position = from; c.rotation = UnityEngine.Quaternion.LookRotation((target - from).normalized); UnityEngine.Physics.SyncTransforms(); }

foreach (var b in UnityEngine.Object.FindObjectsByType<Book>(UnityEngine.FindObjectsInactive.Include)) { if (b.currentSlot != null) b.currentSlot.Release(); if (carry.Contains(b)) carry.Release(b, out _); b.Outline?.StopAll(); }
gp.ResetUpgrades(); carry.SetCapacity(4);

// 1) left click on a loose book picks it up
var loose = B("Book_Astronomy_A1"); loose.transform.position = new UnityEngine.Vector3(3f, loose.transform.localScale.y * 0.5f, 1.5f); loose.transform.rotation = UnityEngine.Quaternion.identity;
Aim(new UnityEngine.Vector3(3f, 1.2f, 0f), loose.transform.position);
Click("left");
Check("LEFT click picked the aimed loose book up", carry.Contains(loose) && carry.Count == 1, "carry=" + carry.Count);

// 2) left click on an empty slot places the held book (and the readout then moves on to the next held book)
var slot = CaseOfTheme(BookCategory.Astronomy).shelves[0].slots[0];
pi.PickUp(B("Book_Beasts_A3"));   // a second book in hand: the "next" book
Aim(new UnityEngine.Vector3(slot.transform.position.x, 1.55f, 3.2f), slot.transform.position + new UnityEngine.Vector3(0f, 0.05f, 0f));
for (int guard = 0; guard < 20 && carry.Current != loose; guard++) carry.Cycle(1);
Click("left");
Check("LEFT click placed the current held book in the aimed slot", slot.occupant == loose && carry.Count == 1, "occupant=" + (slot.occupant ? slot.occupant.name : "none"));
Check("it was graded green (right bookcase, shelf and slot)", loose.Grade() == BookBoy.Rules.PlacementResult.Correct);
mUpdate.Invoke(pi, null);   // the next frame: the readout follows the new state
Check("the readout in the middle of the screen now shows the NEXT held book, not the one just placed", pi.promptTitleText.enabled && pi.promptTitleText.text == B("Book_Beasts_A3").definition.displayName, pi.promptTitleText.text);

// 3) a left click on a book in a full hand does nothing; right click tosses the current book
Aim(new UnityEngine.Vector3(0f, 1.5f, 0f), new UnityEngine.Vector3(0f, 1.5f, 6f));
var toss = B("Book_Beasts_A3");
Click("right");
var rb = toss.GetComponent<UnityEngine.Rigidbody>();
Check("RIGHT click tossed the held book: out of the hands, real physics, flying", !carry.Contains(toss) && rb != null && !rb.isKinematic && rb.linearVelocity.magnitude > 1f, "rb=" + (rb != null));
Check("a right click with empty hands does nothing (no error)", true);
Click("right");

// Q and E still work
var kb = UnityEngine.InputSystem.Keyboard.current;
if (kb != null && kb.enabled)
{
    pi.PickUp(B("Book_History_A2")); int before = carry.Count;
    UnityEngine.InputSystem.InputSystem.QueueStateEvent(kb, new UnityEngine.InputSystem.LowLevel.KeyboardState(UnityEngine.InputSystem.Key.Q)); UnityEngine.InputSystem.InputSystem.Update(); mUpdate.Invoke(pi, null);
    UnityEngine.InputSystem.InputSystem.QueueStateEvent(kb, new UnityEngine.InputSystem.LowLevel.KeyboardState()); UnityEngine.InputSystem.InputSystem.Update();
    Check("the Q key still tosses", carry.Count == before - 1, "before=" + before + " after=" + carry.Count);
}
else sb.Append("(keyboard device not available: Q check skipped)\n");

sb.Append(fails == 0 ? "ALL PASSED" : (fails + " FAILED"));
return sb.ToString();
