using UnityEngine;

/// <summary>Scene-level holder for the materials the book outline glow needs (so they are referenced, and included in builds).</summary>
public class GlowAssets : MonoBehaviour
{
    public Material outlineMaterial;
    public Material sparkleMaterial;

    [Tooltip("Draws a glowing book's silhouette through whatever hides it (sister glow).")]
    public Material xrayMaterial;

    [Tooltip("Additive, vertex-colored material for the Home Beacon's frame lines around a bookcase.")]
    public Material beaconMaterial;
}
