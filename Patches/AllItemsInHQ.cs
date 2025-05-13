extern alias GSD1;
extern alias GSD2;

using HarmonyLib;
using System;

namespace Suikoden_Fix.Patches;

public class AllItemsInHQPatch
{
    static void GSD1_AddAllItems(bool unlockArmor)
    {
        var shopItems = GSD1.OldSrcBase.global_work?.event_c?.syohin_list;
        if (shopItems == null)
        {
            return;
        }

        var itemDatas = GSD1.G_item_c_data.item_data_table;
        if (itemDatas == null)
        {
            return;
        }

        var helpTable = GSD1.G_item_c.item_help_table;
        if (helpTable == null)
        {
            return;
        }

        var length = Math.Min(itemDatas.Count, helpTable.Count); // Items without description crash and are useless anyway

        // No need to reset the shopItems array to 0 since we always set more elements
        for (int itemIndex = 0, shopItemsIndex = 0; itemIndex < length && shopItemsIndex < shopItems.Count; ++itemIndex)
        {
            var itemData = itemDatas[itemIndex];
            if (itemData == null)
            {
                continue;
            }

            var isArmor = (itemData.type & 0x80) != 0;
            if (isArmor != unlockArmor)
            {
                continue;
            }

            shopItems[shopItemsIndex++] = (byte)itemIndex;
        }
    }

    [HarmonyPatch(typeof(GSD1.Event_c), nameof(GSD1.Event_c.shiro_bougu))]
    [HarmonyPostfix]
    static void GSD1_AddAllEquipments()
    {
        GSD1_AddAllItems(true);
    }

    [HarmonyPatch(typeof(GSD1.Event_c), nameof(GSD1.Event_c.shiro_shop))]
    [HarmonyPostfix]
    static void GSD1_AddAllConsumables()
    {
        GSD1_AddAllItems(false);
    }

    [HarmonyPatch(typeof(GSD2.EventOverlayClass.h_dougu), nameof(GSD2.EventOverlayClass.h_dougu.SDUrishinaCheck))]
    [HarmonyPostfix]
    static void GSD2_AddAllItems(GSD2.DOUGUCON dcon)
    {
        var dhdat = dcon?.dhdat;
        if (dhdat == null)
        {
            return;
        }

        var itemDatas = GSD2.OldSrcBase.game_work?.ovit_data;
        if (itemDatas == null)
        {
            return;
        }

        var armorDatas = itemDatas.dogu_data;
        var consumableDatas = itemDatas.bogu_data;
        var shopType = dcon.typ;

        int length;
        if (shopType == 0 && armorDatas != null)
        {
            length = armorDatas.Count;
        }
        else if (shopType == 1 && consumableDatas != null)
        {
            length = consumableDatas.Count;
        }
        else
        {
            return;
        }

        dhdat.Clear();

        for (int i = 0; i < length; ++i)
        {
            if (shopType == 0)
            {
                var itemData = armorDatas[i];
                if (itemData == null ||
                    itemData.name == 1)
                {
                    continue;
                }
            }
            else
            {
                var itemData = consumableDatas[i];
                if (itemData == null ||
                    itemData.name == 1) // empty
                {
                    continue;
                }
            }

            var shopItem = new GSD2.SP_DHDAT
            {
                dhno = (sbyte)i, // item
                mno = 0,
                sno = shopType, // item type
                kosuu = 0 // quantity, 0 = unlimited
            };

            dhdat.Add(shopItem);
        }

        dcon.dhkaz = (sbyte)dhdat.Count;
    }

    [HarmonyPatch(typeof(GSD2.EventOverlayClass.hmonsyo), nameof(GSD2.EventOverlayClass.hmonsyo.SDUrishinaCheck))]
    [HarmonyPostfix]
    static void GSD2_AddAllRunes(GSD2.DOUGUCON dcon)
    {
        var dhdat = dcon?.dhdat;
        if (dhdat == null)
        {
            return;
        }

        var runeDatas = GSD2.OldSrcBase.game_work?.ovit_data?.embl_data;
        if (runeDatas == null)
        {
            return;
        }

        dhdat.Clear();

        for (int i = 0; i < runeDatas.Count; ++i)
        {
            var itemData = runeDatas[i];
            if (itemData == null ||
                itemData.name == 1 || // empty
                itemData.name == 2586 || // empty
                itemData.name == 2607) // empty
            {
                continue;
            }

            var shopItem = new GSD2.SP_DHDAT
            {
                dhno = (sbyte)i, // item
                mno = 0,
                sno = 2, // item type
                kosuu = 0 // quantity, 0 = unlimited
            };

            dhdat.Add(shopItem);
        }

        dcon.dhkaz = (sbyte)dhdat.Count;
    }
}
