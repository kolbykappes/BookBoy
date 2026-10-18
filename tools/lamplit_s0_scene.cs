// Edit Mode eval script. S0: creates Assets/Scenes/Lamplit.unity as a copy of LibraryPrototype.unity with the library content removed
// (bookcases, signs, books, floor, walls, orbs), keeping the player rig, cameras, HUD canvas, the Game object and the global volume.
// The prototype scene is NOT modified. Safe to re-run: if Lamplit.unity already exists it is only re-stripped, never re-copied.
if (UnityEditor.EditorApplication.isPlaying) return "REFUSED: stop Play Mode first";
const string src = "Assets/Scenes/LibraryPrototype.unity";
const string dst = "Assets/Scenes/Lamplit.unity";
var log = new System.Text.StringBuilder();

var active = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
if (active.isDirty) return "REFUSED: save the open scene first (" + active.path + ")";

if (UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEditor.SceneAsset>(dst) == null)
{
    if (!UnityEditor.AssetDatabase.CopyAsset(src, dst)) return "FAILED to copy " + src;
    log.Append("copied; ");
}

var scene = UnityEditor.SceneManagement.EditorSceneManager.OpenScene(dst, UnityEditor.SceneManagement.OpenSceneMode.Single);
log.Append("opened " + scene.path + "; ");

int removed = 0;
foreach (var go in scene.GetRootGameObjects())
{
    string n = go.name;
    bool strip = n.StartsWith("Bookcase_") || n.StartsWith("Sign_") || n == "ScatteredBooks" || n == "Floor" || n == "BackWall" || n == "LeftWall" || n == "RightWall"
        || n.StartsWith("MageLight_") || n == "Hub" || n.StartsWith("Room");
    if (strip) { UnityEngine.Object.DestroyImmediate(go); removed++; }
}
log.Append("removed " + removed + " roots; ");

// shelves/slots that were generated as loose roots
foreach (var sh in UnityEngine.Object.FindObjectsByType<Shelf>(UnityEngine.FindObjectsInactive.Include)) if (sh != null && sh.transform.parent == null) UnityEngine.Object.DestroyImmediate(sh.gameObject);

// the prototype's one-room systems that do not belong here
var cleanup = UnityEngine.GameObject.Find("CleanupTracker");
log.Append("CleanupTracker=" + (cleanup != null) + "; ");

var game = UnityEngine.GameObject.Find("Game");
log.Append("Game components: ");
foreach (var c in game.GetComponents<UnityEngine.Component>()) log.Append(c.GetType().Name + " ");
log.Append("; roots now: ");
foreach (var go in scene.GetRootGameObjects()) log.Append(go.name + ", ");

UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);
UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene);
log.Append("\nsaved.");
return log.ToString();
