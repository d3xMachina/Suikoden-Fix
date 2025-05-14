extern alias GSD1;
extern alias GSD2;

using HarmonyLib;
using System;

namespace Suikoden_Fix.Patches;

public class AllItemsInHQPatch
{
    static void GSD1_AddItems(bool addArmor)
    {
        var blacklistedConsumables = new byte[]
        {
            81, 82, 84, 85, 86, 87, 88, 89, 90, 136, 137, 138, 155, 156, 157, 158, 159, 160, 171, 172, 173, 174, 175, 176, 177, 178, 179
        };

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
            if (isArmor != addArmor)
            {
                continue;
            }

            if (!isArmor &&
                Plugin.Config.AllItemsInHQ.Value == 1 &&
                Array.Exists(blacklistedConsumables, blacklistedIndex => blacklistedIndex == itemIndex))
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
        GSD1_AddItems(true);
    }

    [HarmonyPatch(typeof(GSD1.Event_c), nameof(GSD1.Event_c.shiro_shop))]
    [HarmonyPostfix]
    static void GSD1_AddAllConsumables()
    {
        GSD1_AddItems(false);
    }

    [HarmonyPatch(typeof(GSD2.EventOverlayClass.h_dougu), nameof(GSD2.EventOverlayClass.h_dougu.SDUrishinaCheck))]
    [HarmonyPostfix]
    static void GSD2_AddAllItems(GSD2.DOUGUCON dcon)
    {
        var blacklistedConsumables = new byte[]
        {
            29, 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 48, 49, 50, 51, 52, 53, 54, 55, 56, 57, 58, 59,
            60, 61, 62, 63, 64, 65, 66, 67, 68, 69, 70, 71, 72, 73, 74, 76, 77, 78, 79, 80, 81, 82, 83, 84, 85,
        };

        var alexDishes = new byte[]
        {
            20, 34, 54, 60, 76
        };

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

        var consumableDatas = itemDatas.dogu_data;
        var armorDatas = itemDatas.bogu_data;
        var shopType = dcon.typ;

        int length;
        if (shopType == 0 && consumableDatas != null)
        {
            length = consumableDatas.Count;
        }
        else if (shopType == 1 && armorDatas != null)
        {
            length = armorDatas.Count;
        }
        else
        {
            return;
        }

        dhdat.Clear();

        for (int itemIndex = 0; itemIndex < length; ++itemIndex)
        {
            if (shopType == 0)
            {
                if (Plugin.Config.AllItemsInHQ.Value == 1 &&
                    Array.Exists(blacklistedConsumables, blacklistedIndex => blacklistedIndex == itemIndex))
                {
                    continue;
                }

                var itemData = consumableDatas[itemIndex];
                if (itemData == null ||
                    itemData.name == 1)
                {
                    continue;
                }
            }
            else
            {
                var itemData = armorDatas[itemIndex];
                if (itemData == null ||
                    itemData.name == 1) // empty
                {
                    continue;
                }
            }

            var shopItem = new GSD2.SP_DHDAT
            {
                dhno = (sbyte)itemIndex, // item
                mno = 0, // map ?
                sno = shopType, // item type
                kosuu = 0 // quantity, 0 = unlimited
            };

            dhdat.Add(shopItem);
        }

        // Add Alex dishes
        if (shopType == 0)
        {
            for (int i = 0; i < alexDishes.Length; ++i)
            {
                var shopItem = new GSD2.SP_DHDAT
                {
                    dhno = (sbyte)alexDishes[i], // item
                    mno = 0, // map ?
                    sno = 6, // item type
                    kosuu = 0 // quantity, 0 = unlimited
                };

                dhdat.Add(shopItem);
            }
        }

        dcon.dhkaz = (sbyte)dhdat.Count;
    }

    [HarmonyPatch(typeof(GSD2.EventOverlayClass.hmonsyo), nameof(GSD2.EventOverlayClass.hmonsyo.SDUrishinaCheck))]
    [HarmonyPostfix]
    static void GSD2_AddAllRunes(GSD2.DOUGUCON dcon)
    {
        var blacklistedRunes = new byte[]
        {
            11, 13, 14, 39, 40, 41, 42, 43, 44, 45, 46, 47, 48, 49, 50, 51, 52, 53, 54, 89
        };

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
            if (Plugin.Config.AllItemsInHQ.Value == 1 &&
                Array.Exists(blacklistedRunes, blacklistedIndex => blacklistedIndex == i))
            {
                continue;
            }

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
                mno = 0, // map ?
                sno = 2, // item type
                kosuu = 0 // quantity, 0 = unlimited
            };

            dhdat.Add(shopItem);
        }

        dcon.dhkaz = (sbyte)dhdat.Count;
    }
}
