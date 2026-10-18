// Play Mode check for the four timed abilities (run while playing; part 2 = tools/verify_abilities_part2.cs, >= 12 s later):
// per-ability 60 s timers, recharge upgrades, the timer HUD, Gather Family (2/3/4 books, capacity limit, trail) and Home Beacon.
if (!UnityEditor.EditorApplication.isPlaying) return "REFUSED: enter Play Mode first";
var sb = new System.Text.StringBuilder(); int fails = 0;
void Check(string name, bool ok, string detail = "") { if (!ok) fails++; sb.Append((ok ? "PASS " : "FAIL ") + name + (ok ? "" : "  <-- " + detail) + "\n"); }
Book B(string n) { return UnityEngine.GameObject.Find(n).GetComponent<Book>(); }
Bookcase CaseOfTheme(BookCategory t) { foreach (var bc in UnityEngine.Object.FindObjectsByType<Bookcase>(UnityEngine.FindObjectsInactive.Include)) if (bc.theme == t) return bc; return null; }
var cats = new[] { "Alchemy", "Astronomy", "Beasts", "History" };
string BN(int c, int series, int vol) { return "Book_" + cats[c] + "_" + (series == 0 ? "A" : "B") + vol; }

var game = UnityEngine.GameObject.Find("Game"); var gp = game.GetComponent<GameProgress>(); var ui = game.GetComponent<GameUI>();
var sg = game.GetComponent<SisterGlow>(); var qs = game.GetComponent<QuickShelve>(); var gf = game.GetComponent<GatherFamily>(); var hb = game.GetComponent<HomeBeacon>();
var pl = UnityEngine.GameObject.Find("Player"); var pi = pl.GetComponent<PlayerInteractor>(); var carry = pi.Carry;
if (gf == null || hb == null) return "FAIL: GatherFamily / HomeBeacon components are missing from the Game object";
void Reset() { foreach (var b in UnityEngine.Object.FindObjectsByType<Book>(UnityEngine.FindObjectsInactive.Include)) { if (b.currentSlot != null) b.currentSlot.Release(); if (carry.Contains(b)) carry.Release(b, out _); b.Outline?.StopAll(); } gp.ResetUpgrades(); gp.ResetCooldowns(); carry.SetCapacity(4); }
var tRefresh = typeof(GameUI).GetMethod("RefreshAbilities", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
string RowState(string ability) { var panel = UnityEngine.GameObject.Find("GameUI_Abilities"); var row = panel != null ? panel.transform.Find("Ability_" + ability) : null; return row != null && row.gameObject.activeSelf ? row.Find("State").GetComponent<UnityEngine.UI.Text>().text : null; }

// ---- every ability starts locked, nothing in the timer HUD ----
Reset(); tRefresh.Invoke(ui, null);
Check("fresh run: all four abilities are locked and cost the full 60 s once bought", !gp.Upgrades.HasSisterGlow && !gp.Upgrades.HasQuickShelve && !gp.Upgrades.HasGatherFamily && !gp.Upgrades.HasHomeBeacon && gp.Upgrades.CooldownSeconds(BookBoy.Rules.AbilityId.SisterGlow) == 60f);
Check("fresh run: the timer panel is hidden (no ability unlocked)", UnityEngine.GameObject.Find("GameUI_Abilities") == null || !UnityEngine.GameObject.Find("GameUI_Abilities").activeSelf);
Check("a locked ability refuses and says so (Sister Glow, Gather Family, Home Beacon)", sg.Activate() == 0 && gf.Activate() == 0 && !hb.Activate());

// ---- Sister Glow: own 60 s timer, shown in the HUD ----
gp.AddPoints(40); gp.TryBuy("glow");
pi.PickUp(B(BN(2, 0, 3)));
tRefresh.Invoke(ui, null);
Check("unlocked and unused: Sister Glow is ready and the HUD says READY", gp.IsAbilityReady(BookBoy.Rules.AbilityId.SisterGlow) && (RowState("Sister Glow") ?? "").Contains("READY"), RowState("Sister Glow"));
int lit = sg.Activate(); tRefresh.Invoke(ui, null);
float left = gp.CooldownRemaining(BookBoy.Rules.AbilityId.SisterGlow);
Check("using it lights the sisters and starts a 60 s timer, shown in seconds in the HUD", lit > 0 && left > 59f && left <= 60.01f && (RowState("Sister Glow") ?? "").Contains("60") , "lit=" + lit + " left=" + left + " hud=" + RowState("Sister Glow"));
Check("pressing it again while recharging does nothing", sg.Activate() == 0);
gp.TryBuy("glowcd1");
Check("Glow Recharge I shortens the running timer at once (40 s) and the next cooldown", gp.CooldownRemaining(BookBoy.Rules.AbilityId.SisterGlow) <= 40.01f && gp.Upgrades.CooldownSeconds(BookBoy.Rules.AbilityId.SisterGlow) == 40f);

// ---- each ability has its own timer ----
gp.TryBuy("shelve"); gp.TryBuy("gather"); gp.TryBuy("beacon"); tRefresh.Invoke(ui, null);
Check("the other abilities have their own timers: all ready while Sister Glow recharges", gp.IsAbilityReady(BookBoy.Rules.AbilityId.QuickShelve) && gp.IsAbilityReady(BookBoy.Rules.AbilityId.GatherFamily) && gp.IsAbilityReady(BookBoy.Rules.AbilityId.HomeBeacon) && !gp.IsAbilityReady(BookBoy.Rules.AbilityId.SisterGlow));
Check("the HUD lists all four, three READY and one counting down", (RowState("Quick Shelve") ?? "").Contains("READY") && (RowState("Gather Family") ?? "").Contains("READY") && (RowState("Home Beacon") ?? "").Contains("READY") && !(RowState("Sister Glow") ?? "READY").Contains("READY"), RowState("Sister Glow") + " | " + RowState("Quick Shelve"));

// ---- Home Beacon ----
var home = CaseOfTheme(BookCategory.Light);
Reset(); gp.AddPoints(40); gp.TryBuy("beacon");
pi.PickUp(B("Book_Light_A1"));
Check("Home Beacon lights the bookcase whose theme matches the held book (Healing Magic for the white books)", hb.Activate() && System.Linq.Enumerable.SequenceEqual(hb.LitBookcases, new[] { home }));
var frame = UnityEngine.GameObject.Find("BeaconFrame_" + home.name);
var lines = frame != null ? frame.GetComponentsInChildren<UnityEngine.LineRenderer>() : new UnityEngine.LineRenderer[0];
Check("the glow is a 12-edge frame (a box) with a light", lines.Length == 12 && frame.GetComponentInChildren<UnityEngine.Light>() != null, "edges=" + lines.Length);
var cb = new UnityEngine.Bounds(); bool first = true; foreach (var r in home.GetComponentsInChildren<UnityEngine.Renderer>(true)) { if (first) { cb = r.bounds; first = false; } else cb.Encapsulate(r.bounds); }
var fb = new UnityEngine.Bounds(lines[0].GetPosition(0), UnityEngine.Vector3.zero); foreach (var l in lines) { fb.Encapsulate(l.GetPosition(0)); fb.Encapsulate(l.GetPosition(1)); }
Check("the frame hugs the OUTER bounds of that bookcase (just outside it, not bigger than 10 cm margin)", fb.min.x <= cb.min.x && fb.min.y <= cb.min.y && fb.min.z <= cb.min.z && fb.max.x >= cb.max.x && fb.max.y >= cb.max.y && fb.max.z >= cb.max.z && (fb.size - cb.size).magnitude < 0.2f, "frame " + fb + " vs " + cb);
Check("Home Beacon starts its own timer; pressing again is refused", gp.CooldownRemaining(BookBoy.Rules.AbilityId.HomeBeacon) > 59f && !hb.Activate());
System.AppDomain.CurrentDomain.SetData("beaconRoot", "BeaconFrame_" + home.name);

// ---- Gather Family: nothing happens (and no cooldown) when it cannot pull ----
Reset(); gp.AddPoints(40); gp.TryBuy("gather");
foreach (var n in new[] { BN(2, 0, 3), BN(0, 0, 1), BN(1, 0, 1), BN(3, 0, 1) }) pi.PickUp(B(n));
carry.Cycle(1); for (int guard = 0; guard < 20 && carry.Current != B(BN(2, 0, 3)); guard++) carry.Cycle(1);
Check("hands full: Gather Family pulls nothing and does not start its timer", gf.Activate() == 0 && gp.IsAbilityReady(BookBoy.Rules.AbilityId.GatherFamily));

// ---- Gather Family: starts at 2 books, nearest first, trail behind them ----
Reset(); gp.AddPoints(40); gp.TryBuy("gather");
pi.PickUp(B(BN(2, 0, 3)));   // Beasts A3 in hand; its loose sisters are Beasts A1, A2, A4
var from = pi.interactionCamera.transform.position;
var sisters = new System.Collections.Generic.List<Book> { B(BN(2, 0, 1)), B(BN(2, 0, 2)), B(BN(2, 0, 4)) };
sisters.Sort((a, b) => (a.transform.position - from).sqrMagnitude.CompareTo((b.transform.position - from).sqrMagnitude));
Check("Gather Family starts at 2 books", gp.Upgrades.GatherCount == 2);
int pulled = gf.Activate();
Check("it pulls exactly 2 of the 3 loose sisters (the nearest two) and the hands are reserved for them", pulled == 2 && gf.IncomingCount == 2 && gf.IsBusy, "pulled=" + pulled + " incoming=" + gf.IncomingCount);
Check("it starts its own 60 s timer", gp.CooldownRemaining(BookBoy.Rules.AbilityId.GatherFamily) > 59f);
Check("the first book is already flying with a sparkle trail behind it", sisters[0].transform.Find("GatherTrail") != null && sisters[0].transform.Find("GatherTrail").GetComponent<UnityEngine.ParticleSystem>().isPlaying);
Check("a second press is refused while it recharges", gf.Activate() == 0);
carry.SetCapacity(3); gp.ResetCooldowns();
Check("capacity counts the books already on their way: 1 held + 2 flying fills 3 hands, so nothing more is pulled (and no cooldown used)", gf.Activate() == 0 && gp.IsAbilityReady(BookBoy.Rules.AbilityId.GatherFamily));
carry.SetCapacity(4);
System.AppDomain.CurrentDomain.SetData("gatherPick", new[] { sisters[0].name, sisters[1].name }); System.AppDomain.CurrentDomain.SetData("gatherHeld", BN(2, 0, 3));

// a timer started now, read again in part 2 (the earlier sections reset all timers, so this one is started last)
gp.TryBuy("glow"); gp.StartCooldown(BookBoy.Rules.AbilityId.SisterGlow);
System.AppDomain.CurrentDomain.SetData("glowLeft", (double)gp.CooldownRemaining(BookBoy.Rules.AbilityId.SisterGlow)); System.AppDomain.CurrentDomain.SetData("glowAt", (double)UnityEngine.Time.time);
sb.Append(fails == 0 ? "ALL PASSED (part 1)" : (fails + " FAILED"));
return sb.ToString();
