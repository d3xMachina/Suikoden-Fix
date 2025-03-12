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

                    if (binding.path.Contains("/rightTrigger"))
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
    [HarmonyPatch(typeof(GSD1.h_omise_c), nameof(GSD1.h_omise_c.omise_main))] // Shop
    [HarmonyPatch(typeof(GSD1.h_item_c), nameof(GSD1.h_item_c.item_main))]
    [HarmonyPatch(typeof(GSD1.h_kajiya_c), nameof(GSD1.h_kajiya_c.kajiya_main))] // Shop
    [HarmonyPatch(typeof(GSD1.h_kantei_c), nameof(GSD1.h_kantei_c.kantei_main))] // Shop
    [HarmonyPatch(typeof(GSD1.h_monsyo_c), nameof(GSD1.h_monsyo_c.monsyo_main))] // Shop
    [HarmonyPatch(typeof(GSD1.D_azukar_c), nameof(GSD1.D_azukar_c.azukari_start))] // Storage
    [HarmonyPatch(typeof(GSD1.Event_c), nameof(GSD1.Event_c.teleport_main))] // Teleport
    [HarmonyPatch(typeof(GSD1.D_bath_c), nameof(GSD1.D_bath_c.demo_init))] // Bath
    [HarmonyPatch(typeof(GSD1.D_books_c), nameof(GSD1.D_books_c.demo_init))] // Books
    [HarmonyPatch(typeof(GSD1.D_stone_c), nameof(GSD1.D_stone_c.demo_init))] // Stone plates with characters
    [HarmonyPatch(typeof(GSD1.Teventf4_c), nameof(GSD1.Teventf4_c.member_main))] // Party Add/Remove characters
    //[HarmonyPatch(typeof(GSD1.h_yadoya_c), nameof(GSD1.h_yadoya_c.yadoya_start))] // ???
    //[HarmonyPatch(typeof(GSD1.D_wall_c), nameof(GSD1.D_wall_c.demo_init))] // ???
    [HarmonyPrefix]
    static void GSD1_SpecialMenuEnter()
    {
        ModComponent.Instance.IsInSpecialMenu = true;
    }

    // To find similar hooks, look for references to Q_window_c.QDestroyWindow and functions with "_exit" in their name
    [HarmonyPatch(typeof(GSD1.h_omise_c), nameof(GSD1.h_omise_c.omise_exit))]
    [HarmonyPatch(typeof(GSD1.h_item_c), nameof(GSD1.h_item_c.item_exit))]
    [HarmonyPatch(typeof(GSD1.h_kajiya_c), nameof(GSD1.h_kajiya_c.kajiya_exit))]
    [HarmonyPatch(typeof(GSD1.h_kantei_c), nameof(GSD1.h_kantei_c.kantei_exit))]
    [HarmonyPatch(typeof(GSD1.h_monsyo_c), nameof(GSD1.h_monsyo_c.monsyo_exit))]
    [HarmonyPatch(typeof(GSD1.D_azukar_c), nameof(GSD1.D_azukar_c.azukari_owari))]
    [HarmonyPatch(typeof(GSD1.Event_c), nameof(GSD1.Event_c.teleport_exit))]
    [HarmonyPatch(typeof(GSD1.D_bath_c), nameof(GSD1.D_bath_c.demo_exit))]
    [HarmonyPatch(typeof(GSD1.D_books_c), nameof(GSD1.D_books_c.demo_exit))]
    [HarmonyPatch(typeof(GSD1.D_stone_c), nameof(GSD1.D_stone_c.demo_exit))]
    [HarmonyPatch(typeof(GSD1.D_wall_c), nameof(GSD1.D_wall_c.demo_exit))]
    [HarmonyPatch(typeof(GSD1.Teventf4_c), nameof(GSD1.Teventf4_c.member_main99))]
    //[HarmonyPatch(typeof(GSD1.h_yadoya_c), nameof(GSD1.h_yadoya_c.yadoya_exit))] // ???
    //[HarmonyPatch(typeof(GSD1.D_wall_c), nameof(GSD1.D_wall_c.demo_exit)] // ???
    [HarmonyPrefix]
    static void GSD1_SpecialMenuExit()
    {
        ModComponent.Instance.IsInSpecialMenu = false;
    }
    
    // To find similar hooks, look for references to Window.WindowDelete and functions with "ShopxxxMain" in their name
    [HarmonyPatch(typeof(GSD2.dougu), nameof(GSD2.dougu.ShopDouguMain))]
    [HarmonyPrefix]
    static void GSD2_ShopDouguMain(GSD2.dougu __instance)
    {
        var dcon = __instance.dcon;
        if (dcon != null)
        {
            ModComponent.Instance.IsInSpecialMenu = dcon.step != 'c';
        }
    }
    
    [HarmonyPatch(typeof(GSD2.kaji), nameof(GSD2.kaji.ShopKajiMain))]
    [HarmonyPrefix]
    static void GSD2_ShopKajiMain(GSD2.kaji __instance)
    {
        var dcon = __instance.dcon;
        if (dcon != null)
        {
            ModComponent.Instance.IsInSpecialMenu = dcon.step != 'c';
        }
    }
    
    [HarmonyPatch(typeof(GSD2.kantei), nameof(GSD2.kantei.ShopKanteiMain))]
    [HarmonyPrefix]
    static void GSD2_ShopKanteiMain(GSD2.kantei __instance)
    {
        var dcon = __instance.dcon;
        if (dcon != null)
        {
            ModComponent.Instance.IsInSpecialMenu = dcon.step != 'c';
        }
    }
    
    [HarmonyPatch(typeof(GSD2.koueki), nameof(GSD2.koueki.ShopKouekiMain))]
    [HarmonyPrefix]
    static void GSD2_ShopKouekiMain()
    {
        var kcon = GSD2.koueki.kcon;
        if (kcon != null)
        {
            ModComponent.Instance.IsInSpecialMenu = kcon.step != 'c';
        }
    }
    
    [HarmonyPatch(typeof(GSD2.monsyo), nameof(GSD2.monsyo.ShopMonsyoMain))]
    [HarmonyPrefix]
    static void GSD2_ShopMonsyoMain(GSD2.monsyo __instance)
    {
        var dcon = __instance.dcon;
        if (dcon != null)
        {
            ModComponent.Instance.IsInSpecialMenu = dcon.step != 'c';
        }
    }

    [HarmonyPatch(typeof(GSD2.yado), nameof(GSD2.yado.ShopYadoMain))]
    [HarmonyPrefix]
    static void GSD2_ShopYadoMain(GSD2.yado __instance)
    {
        var dcon = __instance.dcon;
        if (dcon != null)
        {
            ModComponent.Instance.IsInSpecialMenu = dcon.step != 'c';
        }
    }

    // Untested
    [HarmonyPatch(typeof(GSD2.EventOverlayClass.h_souko), nameof(GSD2.EventOverlayClass.h_souko.OverlayBaseSoukoMain))]
    [HarmonyPostfix]
    static void GSD2_OverlayBaseSoukoMain(int __result)
    {
        /*
        var baseSouko = __instance.bsk;
        if (baseSouko != null)
        {
            ModComponent.Instance.IsInShop = baseSouko.stp > 8;
        }
        */
        ModComponent.Instance.IsInSpecialMenu = __result != 1;
    }

    [HarmonyPatch(typeof(GSD1.Co_main_c), nameof(GSD1.Co_main_c.coin_loop))]
    [HarmonyPatch(typeof(GSD1.Go_ws_main), nameof(GSD1.Go_ws_main.go_coin_loop))]
    [HarmonyPatch(typeof(GSD1.Ka_main_c), nameof(GSD1.Ka_main_c.ka_coin_loop))]
    [HarmonyPatch(typeof(GSD1.Ms_ws_main_c), nameof(GSD1.Ms_ws_main_c.ms_coin_loop))]
    [HarmonyPatch(typeof(GSD1.Sd_ws_main_c), nameof(GSD1.Sd_ws_main_c.sd_coin_loop))]
    [HarmonyPatch(typeof(GSD1.Ws_ws_main_c), nameof(GSD1.Ws_ws_main_c.ws_coin_loop))]
    [HarmonyPatch(typeof(GSD1.G1_w_main_c), nameof(GSD1.G1_w_main_c.war_loop))]
    [HarmonyPostfix]
    static void GSD1_Minigame(int __result)
    {
        ModComponent.Instance.IsInSpecialMenu = __result != 0;
    }
}
