using UnityEngine;
using UnityEngine.Rendering.Universal;

/// <summary>
/// A tiny photo studio for the discovery dialog: a copy of a find's model turning slowly in front of a camera that renders into a RenderTexture, lit by its own
/// warm key light, a cool fill and an amber rim. The studio sits 2 km above the world (beyond the main camera's far plane) and its lights have a short range, so
/// nothing of it reaches the game view and nothing of the game reaches it. No layer, renderer feature or project setting is involved. Everything it creates is
/// hidden, not saved, and destroyed with it.
/// </summary>
public class FindPreview : MonoBehaviour
{
    public const int Size = 1024;

    private static readonly Vector3 Origin = new Vector3(0f, 2000f, 0f);

    private GameObject _root;
    private Camera _camera;
    private Transform _pivot;
    private GameObject _model;
    private RenderTexture _texture;
    private Quaternion _pose = Quaternion.identity;
    private float _spin;

    public RenderTexture Texture => _texture;
    public bool HasModel => _model != null;

    /// <summary>The transform of the turning model copy (for tests).</summary>
    public Transform ModelTransform => _model != null ? _model.transform : null;

    /// <summary>Shows a copy of <paramref name="book"/>'s mesh and materials at <paramref name="realSize"/>, resting in <paramref name="poseEuler"/> (front toward the camera).</summary>
    public void Show(Book book, Vector3 realSize, Vector3 poseEuler)
    {
        Build();
        if (_model != null)
        {
            Destroy(_model);
        }

        MeshFilter filter = book.GetComponent<MeshFilter>();
        MeshRenderer source = book.GetComponent<MeshRenderer>();
        _model = new GameObject("FindModel") { hideFlags = HideFlags.HideAndDontSave };
        _model.transform.SetParent(_pivot, false);
        _model.AddComponent<MeshFilter>().sharedMesh = filter.sharedMesh;
        MeshRenderer mr = _model.AddComponent<MeshRenderer>();
        mr.sharedMaterials = source.sharedMaterials;
        mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        mr.receiveShadows = false;
        _model.transform.localScale = realSize;
        _pose = Quaternion.Euler(poseEuler);
        _spin = -28f;   // starts a little turned so the first picture already has depth

        // frame it: the bounding sphere of the box, a narrow lens, a bit of air around it
        float radius = 0.5f * realSize.magnitude;
        const float fov = 26f;
        _camera.fieldOfView = fov;
        float distance = radius / Mathf.Sin(fov * 0.5f * Mathf.Deg2Rad) * 0.84f;
        _camera.transform.localPosition = new Vector3(0f, radius * 0.12f, distance);
        _camera.transform.LookAt(_pivot.position);
        Tick(0f);
    }

    public void Hide()
    {
        if (_model != null)
        {
            Destroy(_model);
            _model = null;
        }

        if (_camera != null)
        {
            _camera.enabled = false;
        }
    }

    /// <summary>Turns the model by <paramref name="unscaledDelta"/> seconds' worth and renders one picture.</summary>
    public void Tick(float unscaledDelta)
    {
        if (_model == null || _camera == null)
        {
            return;
        }

        _spin += unscaledDelta * 26f;
        _model.transform.localRotation = Quaternion.AngleAxis(_spin, Vector3.up) * _pose;
        _camera.Render();
    }

    private void Build()
    {
        if (_root != null)
        {
            return;
        }

        _root = new GameObject("FindPreviewStudio") { hideFlags = HideFlags.HideAndDontSave };
        _root.transform.position = Origin;
        _pivot = new GameObject("Pivot").transform;
        _pivot.SetParent(_root.transform, false);
        _pivot.gameObject.hideFlags = HideFlags.HideAndDontSave;

        _texture = new RenderTexture(Size, Size, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB)
        {
            name = "FindPreview",
            antiAliasing = 4,
            hideFlags = HideFlags.HideAndDontSave
        };
        _texture.Create();

        var camGo = new GameObject("PreviewCamera") { hideFlags = HideFlags.HideAndDontSave };
        camGo.transform.SetParent(_root.transform, false);
        _camera = camGo.AddComponent<Camera>();
        _camera.enabled = false;   // rendered by hand
        _camera.targetTexture = _texture;
        _camera.clearFlags = CameraClearFlags.SolidColor;
        _camera.backgroundColor = new Color(0.13f, 0.09f, 0.06f, 1f);
        _camera.nearClipPlane = 0.02f;
        _camera.farClipPlane = 12f;
        UniversalAdditionalCameraData data = _camera.GetUniversalAdditionalCameraData();
        data.renderPostProcessing = false;
        data.renderShadows = false;
        data.antialiasing = AntialiasingMode.None;

        MakeLight("Key", LightType.Spot, new Vector3(0.9f, 1.0f, 2.2f), new Color(1f, 0.86f, 0.64f), 9f, 9f, 80f);
        MakeLight("Fill", LightType.Point, new Vector3(-1.5f, 0.2f, 1.6f), new Color(0.62f, 0.72f, 1f), 1.5f, 7f, 0f);
        MakeLight("Rim", LightType.Point, new Vector3(0.2f, 0.9f, -1.6f), new Color(1f, 0.70f, 0.38f), 3.5f, 7f, 0f);
    }

    private void MakeLight(string name, LightType type, Vector3 localPosition, Color color, float intensity, float range, float spotAngle)
    {
        var go = new GameObject(name) { hideFlags = HideFlags.HideAndDontSave };
        go.transform.SetParent(_root.transform, false);
        go.transform.localPosition = localPosition;
        go.transform.LookAt(_pivot.position);
        var light = go.AddComponent<Light>();
        light.type = type;
        light.color = color;
        light.intensity = intensity;
        light.range = range;
        light.shadows = LightShadows.None;
        if (type == LightType.Spot)
        {
            light.spotAngle = spotAngle;
        }
    }

    private void OnDestroy()
    {
        if (_root != null)
        {
            Destroy(_root);
        }

        if (_texture != null)
        {
            _texture.Release();
            Destroy(_texture);
        }
    }
}
