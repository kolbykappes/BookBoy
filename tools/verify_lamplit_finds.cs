// Play Mode check for finds and installs (S5): carry a find to its desk spot and the effect applies; wrong spots refuse; the aim prompt
// and a real left click through the Input System work. Run with Lamplit.unity open and playing (Room 1 built).
if (!UnityEditor.EditorApplication.isPlaying) return "REFUSED: enter Play Mode first";
if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().path != "Assets/Scenes/Lamplit.unity") return "REFUSED: Lamplit.unity must be the active scene";
var sb = new System.Text.StringBuilder(); int fails = 0;
void Check(string name, bool ok, string detail = "") { if (!ok) fails++; sb.Append((ok ? "PASS " : "FAIL ") + name + (ok ? "" : "  <-- " + detail) + "\n"); }
var cc = UnityEngine.Object.FindFirstObjectByType<CampaignController>(); var gp = UnityEngine.GameObject.Find("Game").GetComponent<GameProgress>(); var ui = UnityEngine.GameObject.Find("Game").GetComponent<GameUI>();
var pl = UnityEngine.GameObject.Find("Player"); var pi = pl.GetComponent<PlayerInteractor>(); var carry = pi.Carry; var fpc = pl.GetComponent<StarterAssets.FirstPersonController>();
var fl = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
var mUpdate = typeof(PlayerInteractor).GetMethod("Update", fl);
cc.ResetAll();
float baseSpeed = fpc.MoveSpeed;
var apron = cc.FindBook("apron"); var shoes = cc.FindBook("shoes");
var apronSpot = cc.SpotFor("apron_hook"); var shoeSpot = cc.SpotFor("shoe_rack");
void Aim(UnityEngine.Vector3 target) { var c = pi.interactionCamera.transform; var from = new UnityEngine.Vector3(target.x, 1.65f, 2.25f); c.position = from; c.rotation = UnityEngine.Quaternion.LookRotation((target - from).normalized); UnityEngine.Physics.SyncTransforms(); }
UnityEngine.Vector3 AboveSpot(FindSpot s) { return s.transform.position + new UnityEngine.Vector3(0f, 0.25f, 0f); }

Check("fresh: capacity 4, normal speed, nothing found or installed", carry.Capacity == 4 && cc.State.InstalledCount == 0 && cc.State.FoundCount == 0 && gp.Upgrades.CarryCapacity == 4);
Check("the finds start hidden in Room 1, loose and glowing-ready (pick-up-able)", apron != null && shoes != null && apron.currentSlot == null && cc.Room(1).Contains(apron.transform.position));

// ---- the apron ----
Check("pick the apron up: it goes in the hands and is marked found", pi.PickUp(apron) && carry.Contains(apron) && cc.State.IsFound("apron") && !cc.State.IsInstalled("apron"));
cc.JumpTo(0);
Aim(AboveSpot(shoeSpot)); mUpdate.Invoke(pi, null);
Check("aiming at the WRONG spot (shoe rack) with the apron says what it needs", pi.promptTitleText.enabled && pi.promptTitleText.text == "Shoe rack" && pi.promptHintText.text.Contains("Needs the soft-soled shoes"), pi.promptTitleText.text + " / " + pi.promptHintText.text);
Check("pressing install at the wrong spot refuses and keeps the apron", cc.InstallHeld(shoeSpot, carry) == BookBoy.Rules.InstallResult.WrongSpot && carry.Contains(apron) && carry.Capacity == 4);
Aim(AboveSpot(apronSpot)); mUpdate.Invoke(pi, null);
Check("aiming at the RIGHT spot offers to install", pi.promptTitleText.text == "Apron hook" && pi.promptHintText.text.Contains("install the apron"), pi.promptHintText.text);
Check("the crosshair is active on a desk spot", pi.crosshair.dot.rectTransform.sizeDelta.x > 14f);

// a real left click through the Input System installs it
var mouse = UnityEngine.InputSystem.Mouse.current;
UnityEngine.InputSystem.InputSystem.QueueStateEvent(mouse, new UnityEngine.InputSystem.LowLevel.MouseState().WithButton(UnityEngine.InputSystem.LowLevel.MouseButton.Left, true));
UnityEngine.InputSystem.InputSystem.Update(); mUpdate.Invoke(pi, null);
UnityEngine.InputSystem.InputSystem.QueueStateEvent(mouse, new UnityEngine.InputSystem.LowLevel.MouseState()); UnityEngine.InputSystem.InputSystem.Update();
Check("a LEFT click installs the apron: it hangs on the hook, out of the hands", cc.State.IsInstalled("apron") && apronSpot.installed == apron && !carry.Contains(apron) && apron.transform.parent == apronSpot.anchor, "installed=" + cc.State.IsInstalled("apron"));
Check("an installed find cannot be picked up again (its collider is off)", !apron.GetComponent<UnityEngine.Collider>().enabled);
Check("the apron raises the carry capacity from 4 to 6", carry.Capacity == 6 && gp.Upgrades.CarryCapacity == 6, carry.Capacity.ToString());

// ---- the shoes ----
Check("shoes: not installed yet, speed unchanged", !cc.State.IsInstalled("shoes") && System.Math.Abs(fpc.MoveSpeed - baseSpeed) < 0.001f);
pi.PickUp(shoes);
Check("the shoes install in the shoe rack", cc.InstallHeld(shoeSpot, carry) == BookBoy.Rules.InstallResult.Installed && shoeSpot.installed == shoes);
Check("the shoes raise the move speed by 15%", System.Math.Abs(fpc.MoveSpeed - baseSpeed * 1.15f) < 0.01f, fpc.MoveSpeed + " vs " + baseSpeed * 1.15f);
Check("both installed: the HUD counts 2 of 5 finds", cc.State.InstalledCount == 2 && cc.StatsText().Contains("2 / 5"), cc.StatsText());
Check("installing the same find twice is refused", cc.InstallHeld(apronSpot, carry) == BookBoy.Rules.InstallResult.UnknownFind);

// ---- the finds list on Tab ----
ui.OpenUpgrades();
Check("Tab lists the apron and the shoes as INSTALLED and the others as hidden", ui.RowText(0).Contains("INSTALLED") && ui.RowText(1).Contains("INSTALLED") && ui.RowText(2).Contains("hidden in Mailroom") && ui.RowText(4).Contains("hidden in Map Room") && ui.RowText(5).Contains("locked"), ui.RowText(2));
ui.CloseMenu();

// ---- the other abilities stay locked, the dumbwaiter too ----
Check("no ability came on yet (Sister Glow, Home Beacon, Gather Family, Quick Shelve)", !gp.Upgrades.HasSisterGlow && !gp.Upgrades.HasHomeBeacon && !gp.Upgrades.HasGatherFamily && !gp.Upgrades.HasQuickShelve);

// ---- reset gives everything back ----
cc.ResetAll();
Check("reset all: finds back on the table, capacity 4, speed normal, nothing installed", cc.State.InstalledCount == 0 && carry.Capacity == 4 && System.Math.Abs(fpc.MoveSpeed - baseSpeed) < 0.001f && apronSpot.installed == null && cc.Room(1).Contains(apron.transform.position) && apron.GetComponent<UnityEngine.Collider>().enabled && apron.transform.parent != apronSpot.anchor);

sb.Append(fails == 0 ? "ALL PASSED" : (fails + " FAILED"));
return sb.ToString();
