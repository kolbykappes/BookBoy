// Part 2 of the ability check: run >= 12 s after tools/verify_abilities_playmode.cs, still playing.
// Gather Family books have arrived in the hands, the beacon frame expired, timers counted down, and the 3-/4-book pulls.
if (!UnityEditor.EditorApplication.isPlaying) return "REFUSED: enter Play Mode first";
var sb = new System.Text.StringBuilder(); int fails = 0;
void Check(string name, bool ok, string detail = "") { if (!ok) fails++; sb.Append((ok ? "PASS " : "FAIL ") + name + (ok ? "" : "  <-- " + detail) + "\n"); }
Book B(string n) { return UnityEngine.GameObject.Find(n).GetComponent<Book>(); }
var pick = System.AppDomain.CurrentDomain.GetData("gatherPick") as string[]; if (pick == null) return "run part 1 first";
var game = UnityEngine.GameObject.Find("Game"); var gp = game.GetComponent<GameProgress>(); var gf = game.GetComponent<GatherFamily>();
var pl = UnityEngine.GameObject.Find("Player"); var pi = pl.GetComponent<PlayerInteractor>(); var carry = pi.Carry;
var cats = new[] { "Alchemy", "Astronomy", "Beasts", "History" };
string BN(int c, int series, int vol) { return "Book_" + cats[c] + "_" + (series == 0 ? "A" : "B") + vol; }

bool arrived = true; string detail = "";
foreach (var n in pick) { var b = B(n); bool inHands = carry.Contains(b) && b.transform.parent == pi.Carry.carryPoint; arrived &= inHands; detail += " " + n + (inHands ? "=in hands" : "=NOT"); }
Check("the 2 pulled books flew into the hands", arrived && carry.Count == 3 && !gf.IsBusy && gf.IncomingCount == 0, detail + " count=" + carry.Count + " busy=" + gf.IsBusy);
Check("the held book plus the two pulled are all of the same series", carry.Count == 3 && System.Linq.Enumerable.All(carry.Items, b => b.definition.category == BookCategory.Beasts && b.definition.series == 0));
Check("the sparkle trails are cleaned up", System.Linq.Enumerable.All(pick, n => B(n).transform.Find("GatherTrail") == null));
Check("picked-up books are normal carried books again (collider off, parked at the carry point)", System.Linq.Enumerable.All(pick, n => !B(n).GetComponent<UnityEngine.Collider>().enabled));

// ---- timers kept counting down in real time; the beacon frame expired by itself ----
double left0 = (double)System.AppDomain.CurrentDomain.GetData("glowLeft"), at0 = (double)System.AppDomain.CurrentDomain.GetData("glowAt");
float left = gp.CooldownRemaining(BookBoy.Rules.AbilityId.SisterGlow); double elapsed = UnityEngine.Time.time - at0;
Check("an ability's timer counted down by the real time that passed", System.Math.Abs((left0 - left) - elapsed) < 1.0, "was " + left0 + " now " + left + " after " + elapsed.ToString("F1") + " s");
Check("the beacon frame disappeared on its own after its 8 s", UnityEngine.GameObject.Find((string)System.AppDomain.CurrentDomain.GetData("beaconRoot")) == null);

// ---- bigger pulls: 3, then the whole series (only 3 loose sisters exist, so 3) ----
foreach (var b in new System.Collections.Generic.List<Book>(carry.Items)) carry.Release(b, out _);
gp.ResetUpgrades(); gp.ResetCooldowns(); gp.AddPoints(20);
gp.TryBuy("gather"); gp.TryBuy("gather2");
carry.SetCapacity(10);
pi.PickUp(B(BN(1, 0, 2)));   // Astronomy A2: loose sisters A1, A3, A4
Check("Bigger Pull raises it to 3", gp.Upgrades.GatherCount == 3);
int n3 = gf.Activate();
Check("with 3 allowed and 3 loose sisters it pulls all 3", n3 == 3 && gf.IncomingCount == 3, "pulled=" + n3);
System.AppDomain.CurrentDomain.SetData("gather3", true);

sb.Append(fails == 0 ? "ALL PASSED (part 2)" : (fails + " FAILED"));
return sb.ToString();
