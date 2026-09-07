using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(Renderer))]
public class Book : MonoBehaviour
{
    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
    private static readonly int ColorId = Shader.PropertyToID("_Color");

    public BookDefinition definition;

    /// <summary>True once this book has been correctly shelved. Per the MVP spec, a placed
    /// book can no longer be picked up.</summary>
    public bool isPlaced;

    private MaterialPropertyBlock _propertyBlock;

    private void OnEnable()
    {
        ApplyColor();

        // Books are always triggers — never physically block the player's walk. Solid Rigidbody
        // physics for scattered items isn't the goal here (see "tactile reliability over
        // physical realism" in the design principles); this only affects collision response,
        // raycasts still hit triggers by default so pickup/placement detection is unaffected.
        Collider bookCollider = GetComponent<Collider>();
        if (bookCollider != null)
        {
            bookCollider.isTrigger = true;
        }
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
