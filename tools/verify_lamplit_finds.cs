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

// ---- the apron and the shoes are taken, not carried: they go straight to their place and work at once; the discovery dialog tells the story ----
var dlg = ui.GetComponent<FindDialog>();
Check("the discovery dialog exists", dlg != null);
Check("pick the apron up: it is TAKEN (never in the hands), marked found, and installed on its hook at once", pi.PickUp(apron) && !carry.Contains(apron) && cc.State.IsFound("apron") && cc.State.IsInstalled("apron") && apronSpot.installed == apron && apron.transform.parent == apronSpot.anchor);
Check("the discovery dialog opened with the story and the benefit, and the game is paused", dlg.IsOpen && dlg.TitleText == "Uncle Archie's apron" && dlg.StoryText.Contains("Thirty years") && dlg.BenefitText == "Carry 2 more books" && UnityEngine.Time.timeScale == 0f, dlg.TitleText + " / " + UnityEngine.Time.timeScale);
Check("the dialog has a live 3D view of the apron", dlg.Preview != null && dlg.Preview.HasModel && dlg.Preview.Texture != null && dlg.Preview.Texture.width == FindPreview.Size);
dlg.Close();
Check("closing the dialog resumes play", !dlg.IsOpen && UnityEngine.Time.timeScale == 1f);
Check("the installed apron hangs in its hang pose, out of reach (collider off), and the hands are free", !apron.GetComponent<UnityEngine.Collider>().enabled && carry.Count == 0 && UnityEngine.Quaternion.Angle(apron.transform.localRotation, UnityEngine.Quaternion.Euler(apron.definition.hangEuler)) < 0.5f);
Check("the apron raises the carry capacity from 4 to 6", carry.Capacity == 6 && gp.Upgrades.CarryCapacity == 6, carry.Capacity.ToString());

Check("shoes: not taken yet, speed unchanged", !cc.State.IsInstalled("shoes") && System.Math.Abs(fpc.MoveSpeed - baseSpeed) < 0.001f);
Check("taking the shoes installs them on the rack and raises the speed by 15%", pi.PickUp(shoes) && shoeSpot.installed == shoes && !carry.Contains(shoes) && System.Math.Abs(fpc.MoveSpeed - baseSpeed * 1.15f) < 0.01f, fpc.MoveSpeed + " vs " + baseSpeed * 1.15f);
Check("the shoes' dialog is theirs", dlg.IsOpen && dlg.TitleText.Contains("felt-soled shoes") && dlg.BenefitText == "Move 15% faster");
dlg.Close();
Check("both installed: the HUD counts 2 of 5 finds", cc.State.InstalledCount == 2 && cc.StatsText().Contains("2 / 5"), cc.StatsText());
Check("a find already installed cannot be taken again (no second dialog)", !pi.PickUp(apron) || !dlg.IsOpen);
dlg.Close();

// ---- the other finds are still carried to their spot (the reading glasses stand in for the old carry flow) ----
var glasses = cc.FindBook("glasses"); var glassesSpot = cc.SpotFor("glasses_stand");
Check("the reading glasses are picked up into the hands, marked found, not installed, and open no dialog", pi.PickUp(glasses) && carry.Contains(glasses) && cc.State.IsFound("glasses") && !cc.State.IsInstalled("glasses") && !dlg.IsOpen);
cc.JumpTo(0);
Aim(AboveSpot(cc.SpotFor("clipboard"))); mUpdate.Invoke(pi, null);
Check("aiming at the WRONG spot (the empty clipboard) with the glasses says what it needs", pi.promptTitleText.enabled && pi.promptTitleText.text == "Clipboard" && pi.promptHintText.text.Contains("Needs the pull list"), pi.promptTitleText.text + " / " + pi.promptHintText.text);
Check("pressing install at the wrong spot refuses and keeps the glasses", cc.InstallHeld(apronSpot, carry) == BookBoy.Rules.InstallResult.WrongSpot && carry.Contains(glasses));
Aim(AboveSpot(glassesSpot)); mUpdate.Invoke(pi, null);
Check("aiming at the RIGHT spot offers to install", pi.promptTitleText.text == "Glasses stand" && pi.promptHintText.text.Contains("install the reading glasses"), pi.promptHintText.text);
Check("the crosshair is active on a desk spot", pi.crosshair.dot.rectTransform.sizeDelta.x > 14f);
// a real left click through the Input System installs it
var mouse = UnityEngine.InputSystem.Mouse.current;
UnityEngine.InputSystem.InputSystem.QueueStateEvent(mouse, new UnityEngine.InputSystem.LowLevel.MouseState().WithButton(UnityEngine.InputSystem.LowLevel.MouseButton.Left, true));
UnityEngine.InputSystem.InputSystem.Update(); mUpdate.Invoke(pi, null);
UnityEngine.InputSystem.InputSystem.QueueStateEvent(mouse, new UnityEngine.InputSystem.LowLevel.MouseState()); UnityEngine.InputSystem.InputSystem.Update();
bool clicked = cc.State.IsInstalled("glasses");
if (!clicked && !UnityEditorInternal.InternalEditorUtility.isApplicationActive) { sb.Append("NOTE the Editor is unfocused, so the Input System ignored the click (rule 2.2); installing directly instead"); cc.InstallHeld(glassesSpot, carry); }
Check("a LEFT click installs the glasses: they sit on the stand, out of the hands", cc.State.IsInstalled("glasses") && glassesSpot.installed == glasses && !carry.Contains(glasses), "installed=" + cc.State.IsInstalled("glasses"));
Check("installing the same find twice is refused", cc.InstallHeld(glassesSpot, carry) == BookBoy.Rules.InstallResult.UnknownFind);

// ---- the finds list on Tab ----
ui.OpenUpgrades();
Check("Tab lists the apron, the shoes and the glasses as INSTALLED and the others as hidden", ui.RowText(0).Contains("INSTALLED") && ui.RowText(1).Contains("INSTALLED") && ui.RowText(2).Contains("INSTALLED") && ui.RowText(3).Contains("hidden in Mailroom") && ui.RowText(4).Contains("hidden in Map Room") && ui.RowText(5).Contains("locked"), ui.RowText(3));
ui.CloseMenu();

// ---- the other abilities stay locked, the dumbwaiter too ----
Check("Sister Glow came on with the glasses; Home Beacon, Gather Family and Quick Shelve are still locked", gp.Upgrades.HasSisterGlow && !gp.Upgrades.HasHomeBeacon && !gp.Upgrades.HasGatherFamily && !gp.Upgrades.HasQuickShelve);

// ---- reset gives everything back ----
cc.ResetAll();
Check("reset all: finds back on the table, capacity 4, speed normal, nothing installed", cc.State.InstalledCount == 0 && carry.Capacity == 4 && System.Math.Abs(fpc.MoveSpeed - baseSpeed) < 0.001f && apronSpot.installed == null && cc.Room(1).Contains(apron.transform.position) && apron.GetComponent<UnityEngine.Collider>().enabled && apron.transform.parent != apronSpot.anchor && UnityEngine.Quaternion.Angle(apron.transform.rotation, UnityEngine.Quaternion.Euler(apron.definition.tableEuler)) < 0.5f);

sb.Append(fails == 0 ? "ALL PASSED" : (fails + " FAILED"));
return sb.ToString();
