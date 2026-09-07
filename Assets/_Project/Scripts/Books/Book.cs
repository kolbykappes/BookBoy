using System.Collections;
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

    /// <summary>True once the player has dropped this book with real physics (Rigidbody,
    /// tumbling). Keeps OnEnable from forcing the collider back to a trigger after a dropped
    /// book has deliberately been made solid — otherwise a future Editor domain reload would
    /// silently revert it, the same class of bug already hit once with shelf marker colors.</summary>
    [HideInInspector]
    public bool hasPhysicsDrop;

    private MaterialPropertyBlock _propertyBlock;

    private void OnEnable()
    {
        ApplyColor();

        // Books are triggers by default — never physically block the player's walk. Solid
        // Rigidbody physics for scattered/carried items isn't the goal here (see "tactile
        // reliability over physical realism"); this only affects collision response, raycasts
        // still hit triggers by default so pickup/placement detection is unaffected. A dropped
        // book (see PlayerInteractor.Drop) is the one deliberate exception.
        if (!hasPhysicsDrop)
        {
            Collider bookCollider = GetComponent<Collider>();
            if (bookCollider != null)
            {
                bookCollider.isTrigger = true;
            }
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

        SetColor(definition.spineColor);
    }

    /// <summary>Briefly flashes white, then eases back to the book's real spine color — the
    /// feedback for a correct placement.</summary>
    public void PlayPlacedPulse(float totalDuration = 0.4f)
    {
        if (definition == null)
        {
            return;
        }

        StartCoroutine(PulseRoutine(totalDuration));
    }

    private IEnumerator PulseRoutine(float totalDuration)
    {
        float half = totalDuration * 0.5f;

        float t = 0f;
        while (t < half)
        {
            t += Time.deltaTime;
            SetColor(Color.Lerp(definition.spineColor, Color.white, t / half));
            yield return null;
        }

        t = 0f;
        while (t < half)
        {
            t += Time.deltaTime;
            SetColor(Color.Lerp(Color.white, definition.spineColor, t / half));
            yield return null;
        }

        SetColor(definition.spineColor);
    }

    private void SetColor(Color color)
    {
        Renderer bookRenderer = GetComponent<Renderer>();
        if (bookRenderer == null)
        {
            return;
        }

        _propertyBlock ??= new MaterialPropertyBlock();
        bookRenderer.GetPropertyBlock(_propertyBlock);
        // Set both property names so this works whether the shared material uses URP
        // ("_BaseColor") or a legacy/Standard shader ("_Color").
        _propertyBlock.SetColor(BaseColorId, color);
        _propertyBlock.SetColor(ColorId, color);
        bookRenderer.SetPropertyBlock(_propertyBlock);
    }
}
