extern alias GSD1;
extern alias GSD2;

using HarmonyLib;
using UnityEngine.InputSystem;

namespace Suikoden_Fix.Patches;

public class SpeedHackPatch
{
    [HarmonyPatch(typeof(GRInputManager), nameof(GRInputManager.Create))]
    [HarmonyPostfix]
    static void RemoveBinding(GRInputManager __instance, GRInputManager.Mode _mode)
    {
        if (__instance.currentMap == null || __instance.currentMap.Count == 0)
        {
            return;
        }

        foreach (InputActionMap actionMap in __instance.currentMap)
        {
            if (actionMap == null)
            {
                continue;
            }

            foreach (InputAction action in actionMap.actions)
            {
                for (int i = action.bindings.Count - 1; i >= 0; i--)
                {
                    InputBinding binding = action.bindings[i];

                    if (binding.path.Contains("/rightShoulder"))
                    {
                        action.ChangeBinding(i).Erase();
                        Plugin.Log.LogInfo($"Removed binding {binding.path} in action '{action.name}' in map '{actionMap.name}'");
                    }
                    else
                    {
                        //Plugin.Log.LogInfo($"Binding {binding.path} in action '{action.name}' in map '{actionMap.name}'");
                    }
                }
            }
        }
    }

    // To find similar hooks, look for references to Q_window_c.QDispWindow and functions with "_main" in their name
    [HarmonyPatch(typeof(GSD1.h_omise_c), nameof(GSD1.h_omise_c.omise_main))] // shop
    [HarmonyPatch(typeof(GSD1.h_item_c), nameof(GSD1.h_item_c.item_main))]
    [HarmonyPatch(typeof(GSD1.h_kajiya_c), nameof(GSD1.h_kajiya_c.kajiya_main))]
    [HarmonyPatch(typeof(GSD1.h_kantei_c), nameof(GSD1.h_kantei_c.kantei_main))]
    [HarmonyPatch(typeof(GSD1.h_monsyo_c), nameof(GSD1.h_monsyo_c.monsyo_main))]
    [HarmonyPrefix]
    static void GSD1_ShopEnter()
    {
        ModComponent.Instance.IsInShop = true;
    }

    // To find similar hooks, look for references to Q_window_c.QDestroyWindow and functions with "_exit" in their name
    [HarmonyPatch(typeof(GSD1.h_omise_c), nameof(GSD1.h_omise_c.omise_exit))]
    [HarmonyPatch(typeof(GSD1.h_item_c), nameof(GSD1.h_item_c.item_exit))]
    [HarmonyPatch(typeof(GSD1.h_kajiya_c), nameof(GSD1.h_kajiya_c.kajiya_exit))]
    [HarmonyPatch(typeof(GSD1.h_kantei_c), nameof(GSD1.h_kantei_c.kantei_exit))]
    [HarmonyPatch(typeof(GSD1.h_monsyo_c), nameof(GSD1.h_monsyo_c.monsyo_exit))]
    [HarmonyPrefix]
    static void GSD1_ShopExit()
    {
        ModComponent.Instance.IsInShop = false;
    }
    
    // To find similar hooks, look for references to Window.WindowDelete and functions with "ShopxxxMain" in their name
    [HarmonyPatch(typeof(GSD2.dougu), nameof(GSD2.dougu.ShopDouguMain))]
    [HarmonyPrefix]
    static void GSD2_ShopDouguMain(GSD2.dougu __instance)
    {
        var dcon = __instance.dcon;
        if (dcon != null)
        {
            ModComponent.Instance.IsInShop = dcon.step != 'c';
        }
    }
    
    [HarmonyPatch(typeof(GSD2.kaji), nameof(GSD2.kaji.ShopKajiMain))]
    [HarmonyPrefix]
    static void GSD2_ShopKajiMain(GSD2.kaji __instance)
    {
        var dcon = __instance.dcon;
        if (dcon != null)
        {
            ModComponent.Instance.IsInShop = dcon.step != 'c';
        }
    }
    
    [HarmonyPatch(typeof(GSD2.kantei), nameof(GSD2.kantei.ShopKanteiMain))]
    [HarmonyPrefix]
    static void GSD2_ShopKanteiMain(GSD2.kantei __instance)
    {
        var dcon = __instance.dcon;
        if (dcon != null)
        {
            ModComponent.Instance.IsInShop = dcon.step != 'c';
        }
    }
    
    [HarmonyPatch(typeof(GSD2.koueki), nameof(GSD2.koueki.ShopKouekiMain))]
    [HarmonyPrefix]
    static void GSD2_ShopKouekiMain()
    {
        var kcon = GSD2.koueki.kcon;
        if (kcon != null)
        {
            ModComponent.Instance.IsInShop = kcon.step != 'c';
        }
    }
    
    [HarmonyPatch(typeof(GSD2.monsyo), nameof(GSD2.monsyo.ShopMonsyoMain))]
    [HarmonyPrefix]
    static void GSD2_ShopMonsyoMain(GSD2.monsyo __instance)
    {
        var dcon = __instance.dcon;
        if (dcon != null)
        {
            ModComponent.Instance.IsInShop = dcon.step != 'c';
        }
    }

    [HarmonyPatch(typeof(GSD2.yado), nameof(GSD2.yado.ShopYadoMain))]
    [HarmonyPrefix]
    static void GSD2_ShopYadoMain(GSD2.yado __instance)
    {
        var dcon = __instance.dcon;
        if (dcon != null)
        {
            ModComponent.Instance.IsInShop = dcon.step != 'c';
        }
    }
}
