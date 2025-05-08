using HarmonyLib;
using UnityEngine;

namespace Suikoden_Fix.Patches;

public class SmoothSpritesPatch
{
    [HarmonyPatch(typeof(GRSpriteRenderer), nameof(GRSpriteRenderer.material), MethodType.Setter)]
    [HarmonyPatch(typeof(GRSpriteRenderer), nameof(GRSpriteRenderer.Awake))]
    [HarmonyPostfix]
    static void SetTextureFilterMode(GRSpriteRenderer __instance)
    {
        var texture = __instance._mat?.mainTexture;
        if (texture == null)
        {
            return;
        }

        texture.filterMode = FilterMode.Bilinear;
    }
}
