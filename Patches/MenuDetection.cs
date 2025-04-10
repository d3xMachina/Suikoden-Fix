extern alias GSD1;
extern alias GSD2;

using HarmonyLib;

namespace Suikoden_Fix.Patches;

public class MenuDetectionPatch
{
    // To find similar hooks, look for references to Q_window_c.QDispWindow and functions with "_main" in their name
    [HarmonyPatch(typeof(GSD1.h_omise_c), nameof(GSD1.h_omise_c.omise_main))] // Shop
    [HarmonyPatch(typeof(GSD1.h_item_c), nameof(GSD1.h_item_c.item_main))]
    [HarmonyPatch(typeof(GSD1.h_kajiya_c), nameof(GSD1.h_kajiya_c.kajiya_main))] // Blacksmith
    [HarmonyPatch(typeof(GSD1.h_kantei_c), nameof(GSD1.h_kantei_c.kantei_main))] // Appraiser
    [HarmonyPatch(typeof(GSD1.h_monsyo_c), nameof(GSD1.h_monsyo_c.monsyo_main))] // Shop
    [HarmonyPatch(typeof(GSD1.D_azukar_c), nameof(GSD1.D_azukar_c.azukari_start))] // Storage
    [HarmonyPatch(typeof(GSD1.Event_c), nameof(GSD1.Event_c.teleport_main))] // Teleport
    [HarmonyPatch(typeof(GSD1.D_bath_c), nameof(GSD1.D_bath_c.demo_init))] // Bath
    [HarmonyPatch(typeof(GSD1.D_books_c), nameof(GSD1.D_books_c.demo_init))] // Books
    [HarmonyPatch(typeof(GSD1.D_stone_c), nameof(GSD1.D_stone_c.demo_init))] // Stone plates with characters
    [HarmonyPatch(typeof(GSD1.Teventf4_c), nameof(GSD1.Teventf4_c.member_main))] // Party Add/Remove characters
    //[HarmonyPatch(typeof(GSD1.h_yadoya_c), nameof(GSD1.h_yadoya_c.yadoya_start))] // ???
    //[HarmonyPatch(typeof(GSD1.D_wall_c), nameof(GSD1.D_wall_c.demo_init))] // ???
    [HarmonyPatch(typeof(GSD2.EventOverlayClass.book_main), nameof(GSD2.EventOverlayClass.book_main.BookMain))]
    [HarmonyPatch(typeof(GSD2.EventOverlayClass.meyasu_main), nameof(GSD2.EventOverlayClass.meyasu_main.MeyasuboxMain))]
    [HarmonyPatch(typeof(GSD2.EventOverlayClass.p_main), nameof(GSD2.EventOverlayClass.p_main.PeepingMain))]
    [HarmonyPatch(typeof(GSD2.EventOverlayClass.s_main), nameof(GSD2.EventOverlayClass.s_main.StoneMain))]
    [HarmonyPatch(typeof(GSD2.EventOverlayClass.t_main), nameof(GSD2.EventOverlayClass.t_main.TanteiMain))]
    [HarmonyPrefix]
    static void SpecialMenuEnter()
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
    [HarmonyPatch(typeof(GSD2.EventOverlayClass.book_main), nameof(GSD2.EventOverlayClass.book_main.BookEnd))]
    [HarmonyPatch(typeof(GSD2.EventOverlayClass.meyasu_main), nameof(GSD2.EventOverlayClass.meyasu_main.MeyasuboxEnd))]
    [HarmonyPatch(typeof(GSD2.EventOverlayClass.p_main), nameof(GSD2.EventOverlayClass.p_main.PeepingEnd))]
    [HarmonyPatch(typeof(GSD2.EventOverlayClass.s_main), nameof(GSD2.EventOverlayClass.s_main.StoneEnd))]
    [HarmonyPatch(typeof(GSD2.EventOverlayClass.t_main), nameof(GSD2.EventOverlayClass.t_main.TanteiEnd))]
    [HarmonyPrefix]
    static void SpecialMenuExit()
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

    [HarmonyPatch(typeof(GSD2.EventOverlayClass.telepo), nameof(GSD2.EventOverlayClass.telepo.telepoMain))]
    [HarmonyPrefix]
    static void GSD2_TelepoMain(GSD2.EventOverlayClass.telepo __instance)
    {
        var tcon = __instance.tcon;
        if (tcon != null)
        {
            ModComponent.Instance.IsInSpecialMenu = tcon.step != 'c';
        }
    }

    [HarmonyPatch(typeof(GSD2.EventOverlayClass.h_souko), nameof(GSD2.EventOverlayClass.h_souko.OverlayBaseSoukoMain))]
    [HarmonyPatch(typeof(GSD2.EventOverlayClass.h_dougu), nameof(GSD2.EventOverlayClass.h_dougu.ShopDouguMain))] // has dcon
    [HarmonyPatch(typeof(GSD2.EventOverlayClass.h_fudazuk), nameof(GSD2.EventOverlayClass.h_fudazuk.ShopMonsyoMain))] // has dcon
    [HarmonyPatch(typeof(GSD2.EventOverlayClass.h_kaji), nameof(GSD2.EventOverlayClass.h_kaji.ShopKajiMain))] // has dcon
    [HarmonyPatch(typeof(GSD2.EventOverlayClass.h_rest), nameof(GSD2.EventOverlayClass.h_rest.RestMain))]
    [HarmonyPatch(typeof(GSD2.EventOverlayClass.h_shugo), nameof(GSD2.EventOverlayClass.h_shugo.ShugoMain))]
    [HarmonyPatch(typeof(GSD2.EventOverlayClass.hmonsyo), nameof(GSD2.EventOverlayClass.hmonsyo.ShopMonsyoMain))] // has dcon
    [HarmonyPatch(typeof(GSD2.EventOverlayClass.hsound), nameof(GSD2.EventOverlayClass.hsound.HSoundMain))]
    [HarmonyPatch(typeof(GSD2.EventOverlayClass.hwindow), nameof(GSD2.EventOverlayClass.hwindow.HWindowMain))]
    [HarmonyPatch(typeof(GSD2.EventOverlayClass.k_get), nameof(GSD2.EventOverlayClass.k_get.k_OverlayAddItem2Main))]
    [HarmonyPatch(typeof(GSD2.EventOverlayClass.k_get), nameof(GSD2.EventOverlayClass.k_get.KikoriItemGetLoop))]
    [HarmonyPatch(typeof(GSD2.EventOverlayClass.Overlay108Win), nameof(GSD2.EventOverlayClass.Overlay108Win.S108InMain))]
    [HarmonyPatch(typeof(GSD2.EventOverlayClass.Overlay_addeven), nameof(GSD2.EventOverlayClass.Overlay_addeven.OverlayAddEventMain))]
    [HarmonyPatch(typeof(GSD2.EventOverlayClass.Overlay_AddItem2), nameof(GSD2.EventOverlayClass.Overlay_AddItem2.OverlayAddItem2Main))]
    [HarmonyPatch(typeof(GSD2.EventOverlayClass.Overlay_ElvWin), nameof(GSD2.EventOverlayClass.Overlay_ElvWin.ElvWinMain))]
    [HarmonyPatch(typeof(GSD2.EventOverlayClass.Overlay_partychg), nameof(GSD2.EventOverlayClass.Overlay_partychg.PartyChageMain))] // has pcon
    [HarmonyPatch(typeof(GSD2.EventOverlayClass.Overlay_partychg1), nameof(GSD2.EventOverlayClass.Overlay_partychg1.PartyChageMain))] // has pcon
    [HarmonyPatch(typeof(GSD2.EventOverlayClass.OverlayLookMenu), nameof(GSD2.EventOverlayClass.OverlayLookMenu.LookMenuMain))]
    [HarmonyPatch(typeof(GSD2.EventOverlayClass.Overlay_furo), nameof(GSD2.EventOverlayClass.Overlay_furo.furomain))] // Bath
    [HarmonyPatch(typeof(GSD2.EventOverlayClass.sndtest), nameof(GSD2.EventOverlayClass.sndtest.SndTestMain))]
    [HarmonyPatch(typeof(GSD2.EventOverlayClass.syugoitm), nameof(GSD2.EventOverlayClass.syugoitm.s_OverlayAddItem2Main))]
    [HarmonyPatch(typeof(GSD2.Overlay_teamchg), nameof(GSD2.Overlay_teamchg.TeamChgMain))] // War units rearrange
    [HarmonyPostfix]
    static void GSD2_SpecialMenu(int __result)
    {
        ModComponent.Instance.IsInSpecialMenu = __result != 1;
    }

    /*
    [HarmonyPatch(typeof(GSD1.Co_main_c), nameof(GSD1.Co_main_c.coin_loop))]
    [HarmonyPatch(typeof(GSD1.Go_ws_main), nameof(GSD1.Go_ws_main.go_coin_loop))]
    [HarmonyPatch(typeof(GSD1.Ka_main_c), nameof(GSD1.Ka_main_c.ka_coin_loop))]
    [HarmonyPatch(typeof(GSD1.Ms_ws_main_c), nameof(GSD1.Ms_ws_main_c.ms_coin_loop))] // Window sound
    [HarmonyPatch(typeof(GSD1.Sd_ws_main_c), nameof(GSD1.Sd_ws_main_c.sd_coin_loop))]
    [HarmonyPatch(typeof(GSD1.Ws_ws_main_c), nameof(GSD1.Ws_ws_main_c.ws_coin_loop))]
    [HarmonyPatch(typeof(GSD1.G1_w_main_c), nameof(GSD1.G1_w_main_c.war_loop))] // War
    [HarmonyPostfix]
    static void GSD1_Minigame(int __result)
    {
        ModComponent.Instance.IsInSpecialMenu = __result != 0;
    }
    */
}
