// Edit Mode renders of the view-following held stack at a shelf and on open floor, several pitches (negative = looking up).
var d = "C:/Users/angry/AppData/Local/Temp/claude/C--Code-BookBoy/f093f5b7-069b-4f91-9e47-626a41770065/scratchpad/";
var V = new System.Func<float, float, float, UnityEngine.Vector3>((x, y, z) => new UnityEngine.Vector3(x, y, z));
var pl = UnityEngine.GameObject.Find("Player"); var pi = pl.GetComponent<PlayerInteractor>(); var carry = pl.GetComponent<CarryInventory>(); carry.SetCapacity(8);
var cam = pi.interactionCamera.transform;
foreach (var n in new[] { "A1", "B1", "C1" }) carry.TryPickUp(UnityEngine.GameObject.Find("R1_" + n).GetComponent<Book>());
void Go(string tag, UnityEngine.Vector3 pos, float pitchDown)
{
    pl.transform.position = pos; pl.transform.rotation = UnityEngine.Quaternion.Euler(0f, 270f, 0f); UnityEngine.Physics.SyncTransforms();
    cam.position = pos + V(0f, 1.65f, 0f); cam.rotation = UnityEngine.Quaternion.Euler(pitchDown, 270f, 0f);
    carry.SnapStack();
    LamplitBuilder.Shot(d + "L_" + tag + ".png", cam.position, cam.position + cam.forward * 3f, 58.7f, 1920, 1080, 0.1f);
}
Go("shelf_up35", V(-7.5f, 0f, 2.06f), -35f);
Go("shelf_up15", V(-7.5f, 0f, 2.06f), -15f);
Go("shelf_level", V(-7.5f, 0f, 2.06f), 0f);
Go("floor_level", V(-4.0f, 0f, 3.56f), 0f);
Go("floor_down35", V(-4.0f, 0f, 3.56f), 35f);
Go("floor_down60", V(-4.0f, 0f, 3.56f), 60f);
return "ok";
