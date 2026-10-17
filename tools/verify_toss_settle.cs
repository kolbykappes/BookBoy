// Part 2 of the toss check: run ~6 s after verify_carry_playmode.cs while still playing.
if (!UnityEditor.EditorApplication.isPlaying) return "REFUSED: enter Play Mode first";
var name = System.AppDomain.CurrentDomain.GetData("tossedName") as string; if (name == null) return "run verify_carry_playmode.cs first";
var b = UnityEngine.GameObject.Find(name).GetComponent<Book>(); var rb = b.GetComponent<UnityEngine.Rigidbody>(); var col = b.GetComponent<UnityEngine.Collider>();
var pl = UnityEngine.GameObject.Find("Player").transform.position; var p = b.transform.position;
float half = System.Math.Min(b.transform.localScale.x, System.Math.Min(b.transform.localScale.y, b.transform.localScale.z)) * 0.5f;
var sb = new System.Text.StringBuilder(); int fails = 0;
void Check(string n, bool ok, string d = "") { if (!ok) fails++; sb.Append((ok ? "PASS " : "FAIL ") + n + (ok ? "" : "  <-- " + d) + "\n"); }
Check("the tossed book has settled: no rigidbody left, back to a trigger, hasPhysicsDrop cleared", rb == null && col.isTrigger && !b.hasPhysicsDrop, "rb=" + (rb != null) + " trigger=" + col.isTrigger);
Check("it landed on the floor (not through it, not floating)", p.y > -0.01f && p.y < 0.4f, "y=" + p.y.ToString("F3"));
Check("it landed inside the room", System.Math.Abs(p.x) < 7.7f && p.z > -5.7f && p.z < 5.7f, p.ToString("F2"));
Check("it is not inside the player", UnityEngine.Vector2.Distance(new UnityEngine.Vector2(p.x, p.z), new UnityEngine.Vector2(pl.x, pl.z)) > 0.3f, p.ToString("F2") + " player " + pl.ToString("F2"));
var q = b.transform.eulerAngles; sb.Append("landed at " + p.ToString("F2") + " rotation " + q.ToString("F0") + " (resting on " + (System.Math.Abs(UnityEngine.Vector3.Dot(b.transform.up, UnityEngine.Vector3.up)) > 0.9f ? "its edge/standing" : "a face") + ")\n");
sb.Append(fails == 0 ? "ALL PASSED (part 2)" : (fails + " FAILED"));
return sb.ToString();
