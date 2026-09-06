using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(Renderer))]
public class Book : MonoBehaviour
{
    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
    private static readonly int ColorId = Shader.PropertyToID("_Color");

    public BookDefinition definition;

    private MaterialPropertyBlock _propertyBlock;

    private void OnEnable()
    {
        ApplyColor();
    }

    private void OnValidate()
    {
        ApplyColor();
    }

    /// <summary>
    /// Re-applies the spine color via a MaterialPropertyBlock, so every book can show its own
    /// color while still sharing one base material — no per-object material instances, no
    /// leaks in edit mode. Call this after assigning <see cref="definition"/> from code (e.g.
    /// editor tooling) — a plain field assignment does not trigger OnValidate.
    /// </summary>
    public void ApplyColor()
    {
        if (definition == null)
        {
            return;
        }

        Renderer bookRenderer = GetComponent<Renderer>();
        if (bookRenderer == null)
        {
            return;
        }

        _propertyBlock ??= new MaterialPropertyBlock();
        bookRenderer.GetPropertyBlock(_propertyBlock);
        // Set both property names so this works whether the shared material uses URP
        // ("_BaseColor") or a legacy/Standard shader ("_Color").
        _propertyBlock.SetColor(BaseColorId, definition.spineColor);
        _propertyBlock.SetColor(ColorId, definition.spineColor);
        bookRenderer.SetPropertyBlock(_propertyBlock);
    }
}
