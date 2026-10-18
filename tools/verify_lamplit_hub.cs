// Play Mode check for the Lamplit hub (S3/S5 desk wiring). Run with Lamplit.unity open and playing.
if (!UnityEditor.EditorApplication.isPlaying) return "REFUSED: enter Play Mode first";
if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().path != "Assets/Scenes/Lamplit.unity") return "REFUSED: Lamplit.unity must be the active scene";
var sb = new System.Text.StringBuilder(); int fails = 0;
void Check(string name, bool ok, string detail = "") { if (!ok) fails++; sb.Append((ok ? "PASS " : "FAIL ") + name + (ok ? "" : "  <-- " + detail) + "\n"); }

var cc = UnityEngine.Object.FindFirstObjectByType<CampaignController>();
Check("a CampaignController exists in the scene", cc != null);
if (cc == null) return sb.ToString() + "FAILED";
var pl = UnityEngine.GameObject.Find("Player"); var pi = pl.GetComponent<PlayerInteractor>();
var ui = UnityEngine.GameObject.Find("Game").GetComponent<GameUI>(); var gp = UnityEngine.GameObject.Find("Game").GetComponent<GameProgress>();

Check("3 doors and 6 desk spots are registered", cc.doors.Length == 3 && cc.spots.Length == 6, cc.doors.Length + " doors, " + cc.spots.Length + " spots");
Check("the player starts at the desk end of the hub, on the floor", System.Math.Abs(pl.transform.position.x) < 0.1f && pl.transform.position.z > 2.5f && pl.transform.position.z < 5f && UnityEngine.Physics.Raycast(pl.transform.position, UnityEngine.Vector3.down, 1.3f), pl.transform.position.ToString());
Check("Room 1's door is open, Rooms 2 and 3 are shut (their blockers are solid)", cc.doors[0].IsOpen && !cc.doors[1].IsOpen && !cc.doors[2].IsOpen && !cc.doors[0].blocker.enabled && cc.doors[1].blocker.enabled && cc.doors[2].blocker.enabled);
Check("shut doors say LOCKED, the open one does not", cc.doors[1].label.text.Contains("LOCKED") && cc.doors[2].label.text.Contains("LOCKED") && !cc.doors[0].label.text.Contains("LOCKED"), cc.doors[0].label.text + " | " + cc.doors[1].label.text);

// the hallway can be walked end to end (a player-sized capsule sweeps down it without hitting anything solid)
var start = new UnityEngine.Vector3(0f, 0.45f, 4.8f); var top = new UnityEngine.Vector3(0f, 1.5f, 4.8f);   // just past the player capsule
bool blocked = UnityEngine.Physics.CapsuleCast(start, top, 0.4f, UnityEngine.Vector3.forward, out var hit, 19f, ~0, UnityEngine.QueryTriggerInteraction.Ignore);
Check("the hallway is clear from the player to the far end", !blocked, blocked ? "blocked by " + hit.collider.name + " at z=" + hit.point.z.ToString("F1") : "");
Check("the player cannot walk through a shut door (Room 2's leaf stops a sweep to the right at its door)", UnityEngine.Physics.CapsuleCast(new UnityEngine.Vector3(0f, 0.45f, cc.doors[1].leaf.position.z), new UnityEngine.Vector3(0f, 1.5f, cc.doors[1].leaf.position.z), 0.4f, UnityEngine.Vector3.right, 3.5f, ~0, UnityEngine.QueryTriggerInteraction.Ignore));
Check("Room 1's doorway is open (a sweep to the left at its door is not stopped by the leaf)", !UnityEngine.Physics.CapsuleCast(new UnityEngine.Vector3(0f, 0.45f, cc.doors[0].leaf.position.z), new UnityEngine.Vector3(0f, 1.5f, cc.doors[0].leaf.position.z), 0.4f, UnityEngine.Vector3.left, out var h1, 2.0f, ~0, UnityEngine.QueryTriggerInteraction.Ignore) || h1.collider.name != "Leaf");

// each desk spot can be aimed at from where the player starts
var eye = new UnityEngine.Vector3(0f, 1.15f, 3.6f); int aimed = 0; string bad = "";
foreach (var s in cc.spots)
{
    var target = s.transform.position + new UnityEngine.Vector3(0f, 0.25f, 0f);
    var hits = UnityEngine.Physics.RaycastAll(eye, (target - eye).normalized, 9f, ~0, UnityEngine.QueryTriggerInteraction.Collide);
    System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
    FindSpot first = null; foreach (var h in hits) { first = h.collider.GetComponentInParent<FindSpot>(); break; }
    if (first == s) aimed++; else bad += " " + s.spotId + "->" + (hits.Length > 0 ? hits[0].collider.name : "none");
}
Check("all 6 spots are the first thing the aim ray meets", aimed == 6, bad);

// desk behaviour with nothing in hand
var carry = pi.Carry; foreach (var b in new System.Collections.Generic.List<Book>(carry.Items)) carry.Release(b, out _);
var apron = cc.SpotFor("apron_hook"); var dumb = cc.SpotFor(BookBoy.Rules.CampaignCatalog.DumbwaiterSpot);
Check("an empty-handed install attempt installs nothing", cc.InstallHeld(apron, carry) == BookBoy.Rules.InstallResult.UnknownFind && cc.State.InstalledCount == 0);
Check("the dumbwaiter hatch is locked", cc.InstallHeld(dumb, carry) == BookBoy.Rules.InstallResult.LockedSpot && dumb.locked);

// HUD in the hub, and Tab shows the finds (not the tech tree)
typeof(GameUI).GetMethod("Update", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).Invoke(ui, null);
Check("the HUD stats describe the hub", cc.StatsText().Contains("THE LAMP") && cc.StatsText().Contains("FINDS"), cc.StatsText());
Check("every ability starts locked", !gp.Upgrades.HasSisterGlow && !gp.Upgrades.HasHomeBeacon && !gp.Upgrades.HasGatherFamily && !gp.Upgrades.HasQuickShelve && gp.Upgrades.CarryCapacity == 4);
ui.OpenUpgrades();
Check("Tab opens the finds list in a Lamplit scene", ui.FindsOpen && ui.RowText(0).Contains("Apron") && ui.RowText(5).Contains("Dumbwaiter"), ui.RowText(0));
ui.CloseMenu();
Check("shelves earn no upgrade points in Lamplit", gp.Upgrades.Points == 0);

sb.Append(fails == 0 ? "ALL PASSED" : (fails + " FAILED"));
return sb.ToString();
