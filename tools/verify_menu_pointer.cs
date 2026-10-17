// Play Mode check: opening a menu (Tab) must NOT free the Windows cursor. It stays locked and hidden, and the menu uses its own
// pointer moved by the mouse. Synchronous: queues Input System events and runs GameUI.Update by hand, so it needs no running frames.
if (!UnityEditor.EditorApplication.isPlaying) return "REFUSED: enter Play Mode first";
var sb = new System.Text.StringBuilder(); int fails = 0;
void Check(string name, bool ok, string detail = "") { if (!ok) fails++; sb.Append((ok ? "PASS " : "FAIL ") + name + (ok ? "" : "  <-- " + detail) + "\n"); }
var game = UnityEngine.GameObject.Find("Game"); var gp = game.GetComponent<GameProgress>(); var ui = game.GetComponent<GameUI>();
var pl = UnityEngine.GameObject.Find("Player"); var cursor = pl.GetComponent<CursorLock>();
var fl = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
var uiUpdate = typeof(GameUI).GetMethod("Update", fl);
var kb = UnityEngine.InputSystem.Keyboard.current; var mouse = UnityEngine.InputSystem.Mouse.current;
if (kb == null || mouse == null) return "FAIL: no keyboard/mouse device";
UnityEngine.Transform Pointer() { return UnityEngine.GameObject.Find("HUD_Canvas").transform.Find("GameUI_Pointer"); }
void Frame() { UnityEngine.InputSystem.InputSystem.Update(); uiUpdate.Invoke(ui, null); }
void Release() { UnityEngine.InputSystem.InputSystem.QueueStateEvent(kb, new UnityEngine.InputSystem.LowLevel.KeyboardState()); UnityEngine.InputSystem.InputSystem.QueueStateEvent(mouse, new UnityEngine.InputSystem.LowLevel.MouseState()); UnityEngine.InputSystem.InputSystem.Update(); }

if (ui.MenuOpen) ui.CloseMenu();
gp.ResetUpgrades(); gp.AddPoints(3); Release();
Check("playing: the cursor is locked and hidden", UnityEngine.Cursor.lockState == UnityEngine.CursorLockMode.Locked && !UnityEngine.Cursor.visible);
Check("no pointer is drawn while playing", Pointer() != null && !Pointer().gameObject.activeSelf);

// Tab opens the upgrade menu
UnityEngine.InputSystem.InputSystem.QueueStateEvent(kb, new UnityEngine.InputSystem.LowLevel.KeyboardState(UnityEngine.InputSystem.Key.Tab)); Frame(); Release();
Check("Tab opens the upgrade menu", ui.UpgradesOpen && UnityEngine.Time.timeScale == 0f);
Check("the Windows cursor stays LOCKED and HIDDEN with the menu open (it does not break out of the game)", UnityEngine.Cursor.lockState == UnityEngine.CursorLockMode.Locked && !UnityEngine.Cursor.visible && cursor.suspended, UnityEngine.Cursor.lockState + " visible=" + UnityEngine.Cursor.visible);
Check("the menu's own pointer is drawn instead, starting in the middle of the screen", Pointer().gameObject.activeSelf && System.Math.Abs(ui.VirtualCursor.x - UnityEngine.Screen.width / 2f) < 2f && System.Math.Abs(ui.VirtualCursor.y - UnityEngine.Screen.height / 2f) < 2f, ui.VirtualCursor.ToString());

// the pointer follows mouse movement and cannot leave the screen
var before = ui.VirtualCursor; ui.MoveVirtualCursor(new UnityEngine.Vector2(40f, -25f));
Check("mouse movement moves the pointer", (ui.VirtualCursor - before - new UnityEngine.Vector2(40f, -25f)).magnitude < 0.01f);
ui.MoveVirtualCursor(new UnityEngine.Vector2(99999f, 99999f));
Check("the pointer is clamped to the screen (top-right corner)", ui.VirtualCursor.x <= UnityEngine.Screen.width && ui.VirtualCursor.y <= UnityEngine.Screen.height && ui.VirtualCursor.x > UnityEngine.Screen.width - 3f);
ui.MoveVirtualCursor(new UnityEngine.Vector2(-99999f, -99999f));
Check("... and bottom-left corner", ui.VirtualCursor.x >= 0f && ui.VirtualCursor.y >= 0f && ui.VirtualCursor.x < 3f && ui.VirtualCursor.y < 3f);

// click a row with the pointer: aim it at row 0 (Strong Arms I, 1 point) and press the left button
var row = UnityEngine.GameObject.Find("HUD_Canvas").transform.Find("GameUI_Menu/Row0").GetComponent<UnityEngine.RectTransform>();
var corners = new UnityEngine.Vector3[4]; row.GetWorldCorners(corners);
var centre = (corners[0] + corners[2]) * 0.5f;
ui.MoveVirtualCursor(new UnityEngine.Vector2(centre.x, centre.y) - ui.VirtualCursor);
Check("the pointer is now over row 0 (screen position matches the row)", (ui.VirtualCursor - new UnityEngine.Vector2(centre.x, centre.y)).magnitude < 1f);
UnityEngine.InputSystem.InputSystem.QueueStateEvent(mouse, new UnityEngine.InputSystem.LowLevel.MouseState().WithButton(UnityEngine.InputSystem.LowLevel.MouseButton.Left, true)); Frame(); Release();
Check("a LEFT click with the pointer on a row buys it (Strong Arms I)", gp.Upgrades.IsUnlocked("arms1") && gp.Upgrades.Points == 2, "unlocked=" + gp.Upgrades.IsUnlocked("arms1") + " points=" + gp.Upgrades.Points);

// the admin menu behaves the same
UnityEngine.InputSystem.InputSystem.QueueStateEvent(kb, new UnityEngine.InputSystem.LowLevel.KeyboardState(UnityEngine.InputSystem.Key.F10)); Frame(); Release();
Check("F10 switches to the admin menu, cursor still locked and hidden", ui.AdminOpen && UnityEngine.Cursor.lockState == UnityEngine.CursorLockMode.Locked && !UnityEngine.Cursor.visible);

// Esc closes: pointer gone, play resumes
UnityEngine.InputSystem.InputSystem.QueueStateEvent(kb, new UnityEngine.InputSystem.LowLevel.KeyboardState(UnityEngine.InputSystem.Key.Escape)); Frame(); Release();
Check("Esc closes the menu: time runs, pointer hidden, cursor still locked and hidden", !ui.MenuOpen && UnityEngine.Time.timeScale == 1f && !Pointer().gameObject.activeSelf && UnityEngine.Cursor.lockState == UnityEngine.CursorLockMode.Locked && !UnityEngine.Cursor.visible && !cursor.suspended);

// a click while a menu is open must never hand the cursor back to Windows
ui.OpenUpgrades(); var cursorUpdate = typeof(CursorLock).GetMethod("Update", fl);
UnityEngine.Cursor.lockState = UnityEngine.CursorLockMode.None; UnityEngine.Cursor.visible = true;   // pretend the OS took the lock away (e.g. alt-tab)
cursorUpdate.Invoke(cursor, null);
Check("if the lock is lost while a menu is open it is re-taken", UnityEngine.Cursor.lockState == UnityEngine.CursorLockMode.Locked && !UnityEngine.Cursor.visible);
ui.CloseMenu();
sb.Append(fails == 0 ? "ALL PASSED" : (fails + " FAILED"));
return sb.ToString();
