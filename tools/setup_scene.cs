// Edit Mode eval script (run after tools/setup_assets.cs): lighting rig, tiled floor, walls, and the new ability components.
// Re-runnable: old orbs are removed first; components are only added if missing. Save the scene afterwards.
if (UnityEditor.EditorApplication.isPlaying) return "REFUSED: stop Play Mode first";

var log = new System.Text.StringBuilder();
const string matDir = "Assets/_Project/Art/Materials";
var floorMat = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Material>(matDir + "/Mat_FloorTile.mat");
var wallMat = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Material>(matDir + "/Mat_Wall.mat");
var orbMat = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Material>(matDir + "/Mat_Orb.mat");
var beaconMat = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Material>(matDir + "/Mat_Beacon.mat");
if (floorMat == null || wallMat == null || orbMat == null || beaconMat == null) return "REFUSED: run tools/setup_assets.cs first";

// ---------------------------------------------------------------- the old single directional light becomes a dim, shadowless moon fill
var sun = UnityEngine.GameObject.Find("Directional Light").GetComponent<UnityEngine.Light>();
sun.name = "Directional Light";
sun.color = new UnityEngine.Color(0.62f, 0.70f, 1f);
sun.intensity = 0.45f;
sun.shadows = UnityEngine.LightShadows.None;
UnityEditor.EditorUtility.SetDirty(sun);

// ---------------------------------------------------------------- ambient: a soft gradient instead of the daylight skybox
UnityEngine.RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
UnityEngine.RenderSettings.ambientSkyColor = new UnityEngine.Color(0.30f, 0.27f, 0.34f);
UnityEngine.RenderSettings.ambientEquatorColor = new UnityEngine.Color(0.21f, 0.18f, 0.17f);
UnityEngine.RenderSettings.ambientGroundColor = new UnityEngine.Color(0.08f, 0.065f, 0.05f);
UnityEngine.RenderSettings.ambientIntensity = 1f;
// the room has no ceiling: show a dark night sky behind it instead of a bright daytime one
foreach (var cam in UnityEngine.Object.FindObjectsByType<UnityEngine.Camera>(UnityEngine.FindObjectsInactive.Include))
{
    cam.clearFlags = UnityEngine.CameraClearFlags.SolidColor;
    cam.backgroundColor = new UnityEngine.Color(0.035f, 0.03f, 0.055f);
    UnityEditor.EditorUtility.SetDirty(cam);
}

// ---------------------------------------------------------------- floating magelights: four warm orbs, two of them casting soft shadows
foreach (var old in UnityEngine.Object.FindObjectsByType<MageLight>(UnityEngine.FindObjectsInactive.Include))
    UnityEngine.Object.DestroyImmediate(old.gameObject);

var orbSpecs = new[]
{
    // name, position, intensity, shadows
    new { n = "MageLight_1", p = new UnityEngine.Vector3(-2.6f, 3.0f, 3.3f), i = 4.6f, s = true },
    new { n = "MageLight_2", p = new UnityEngine.Vector3(2.6f, 3.0f, 3.3f), i = 4.6f, s = true },
    new { n = "MageLight_3", p = new UnityEngine.Vector3(-5.2f, 3.4f, -0.5f), i = 4.0f, s = false },
    new { n = "MageLight_4", p = new UnityEngine.Vector3(5.2f, 3.4f, -0.5f), i = 4.0f, s = false },
};
foreach (var spec in orbSpecs)
{
    var orb = UnityEngine.GameObject.CreatePrimitive(UnityEngine.PrimitiveType.Sphere);
    orb.name = spec.n;
    orb.transform.position = spec.p;
    orb.transform.localScale = UnityEngine.Vector3.one * 0.24f;
    UnityEngine.Object.DestroyImmediate(orb.GetComponent<UnityEngine.Collider>());
    var mr = orb.GetComponent<UnityEngine.MeshRenderer>();
    mr.sharedMaterial = orbMat;
    mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
    mr.receiveShadows = false;

    var lightGo = new UnityEngine.GameObject("Light");
    lightGo.transform.SetParent(orb.transform, false);
    lightGo.transform.localScale = UnityEngine.Vector3.one / 0.24f;   // keep the light's own scale at 1 under the shrunken orb
    var light = lightGo.AddComponent<UnityEngine.Light>();
    light.type = UnityEngine.LightType.Point;
    light.color = new UnityEngine.Color(1f, 0.82f, 0.58f);
    light.intensity = spec.i;
    light.range = 13f;
    light.shadows = spec.s ? UnityEngine.LightShadows.Soft : UnityEngine.LightShadows.None;
    if (spec.s) { light.shadowStrength = 0.72f; light.shadowBias = 0.03f; light.shadowNormalBias = 0.5f; }
    orb.AddComponent<MageLight>();
}
log.Append("orbs=" + orbSpecs.Length + "; ");

// ---------------------------------------------------------------- floor and walls
var floorRenderer = UnityEngine.GameObject.Find("Floor").GetComponent<UnityEngine.Renderer>();
floorRenderer.sharedMaterial = floorMat;
UnityEditor.EditorUtility.SetDirty(floorRenderer);
foreach (var wallName in new[] { "BackWall", "LeftWall", "RightWall" })
{
    var r = UnityEngine.GameObject.Find(wallName).GetComponent<UnityEngine.Renderer>();
    r.sharedMaterial = wallMat;
    UnityEditor.EditorUtility.SetDirty(r);
}

// ---------------------------------------------------------------- new ability components on the Game object
var game = UnityEngine.GameObject.Find("Game");
if (game.GetComponent<GatherFamily>() == null) game.AddComponent<GatherFamily>();
if (game.GetComponent<HomeBeacon>() == null) game.AddComponent<HomeBeacon>();
if (game.GetComponent<AdminTools>() == null) game.AddComponent<AdminTools>();
var glowAssets = UnityEngine.Object.FindFirstObjectByType<GlowAssets>();
glowAssets.beaconMaterial = beaconMat;
UnityEditor.EditorUtility.SetDirty(glowAssets);
UnityEditor.EditorUtility.SetDirty(game);
log.Append("game components: gather=" + (game.GetComponent<GatherFamily>() != null) + " beacon=" + (game.GetComponent<HomeBeacon>() != null) + " admin=" + (game.GetComponent<AdminTools>() != null) + "; beaconMat=" + (glowAssets.beaconMaterial != null));

UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
return log.ToString();
