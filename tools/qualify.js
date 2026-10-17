// Expands short Unity type names so a .src.cs script runs under `unity command eval_file`
// (no `using` directives there). Usage: node tools/qualify.js   (reads/writes in tools/)
const fs = require("fs");
const path = require("path");

const map = {
  "UnityEngine": ["GameObject", "MeshRenderer", "TextMesh", "TextAnchor", "TextAlignment", "Resources", "Font", "Material",
    "Vector2", "Vector3", "Quaternion", "Color", "Transform", "PrimitiveType", "BoxCollider", "Collider", "ScriptableObject",
    "FindObjectsInactive", "Sprite", "RectTransform", "Canvas", "RenderMode", "FontStyle"],
  "UnityEditor": ["EditorApplication", "AssetDatabase", "EditorUtility"],
  "UnityEngine.Rendering": ["ShadowCastingMode"],
  "System.Collections.Generic": ["List", "Dictionary"],
};

for (const f of fs.readdirSync(__dirname).filter(n => n.endsWith(".src.cs"))) {
  let s = fs.readFileSync(path.join(__dirname, f), "utf8");
  for (const [ns, names] of Object.entries(map)) {
    for (const n of names) {
      // not already qualified (preceded by '.' or a word char) and not part of an identifier
      s = s.replace(new RegExp("(?<![\\w.])" + n + "(?![\\w])", "g"), ns + "." + n);
    }
  }
  s = s.replace(/(?<![\w.])Object\./g, "UnityEngine.Object.");
  s = s.replace(/^\/\/ SOURCE for.*\n\/\/ qualify step.*\n/m, "// GENERATED from " + f + " by tools/qualify.js - edit the .src.cs, not this file.\n");
  fs.writeFileSync(path.join(__dirname, f.replace(".src.cs", ".cs")), s);
  console.log("wrote", f.replace(".src.cs", ".cs"));
}
