// Edit Mode renders of the held stack at several counts and pitches (the stack builds as books are picked up).
var d = "C:/Users/angry/AppData/Local/Temp/claude/C--Code-BookBoy/f093f5b7-069b-4f91-9e47-626a41770065/scratchpad/";
var V = new System.Func<float, float, float, UnityEngine.Vector3>((x, y, z) => new UnityEngine.Vector3(x, y, z));
var sb = new System.Text.StringBuilder();
var pl = UnityEngine.GameObject.Find("Player"); var carry = pl.GetComponent<CarryInventory>(); carry.SetCapacity(8);
pl.transform.position = V(-4.0f, 0f, 3.56f); pl.transform.rotation = UnityEngine.Quaternion.Euler(0f, 270f, 0f); UnityEngine.Physics.SyncTransforms();
var eye = pl.transform.position + V(0f, 1.65f, 0f); var fwd = pl.transform.forward;
var names = new[] { "A1", "A2", "A3", "B1", "B2", "C1" };
for (int i = 0; i < names.Length; i++)
{
    carry.TryPickUp(UnityEngine.GameObject.Find("R1_" + names[i]).GetComponent<Book>()); carry.SnapStack();
    int n = carry.Count;
    foreach (float pitch in new[] { 35f })
    {
        if (true)
            LamplitBuilder.Shot(d + "K_n" + n + "_p" + (int)pitch + ".png", eye, eye + UnityEngine.Quaternion.AngleAxis(pitch, pl.transform.right) * fwd * 3f, 58.7f, 1920, 1080);
    }
}
return "ok";
