using HarmonyLib;
using UnityEngine.InputSystem;

namespace Suikoden_Fix.Patches;

public class FixKeyboardBindingsPatch
{
    [HarmonyPatch(typeof(GRInputManager), nameof(GRInputManager.Create))]
    [HarmonyPostfix]
    static void AddKeybind(GRInputManager __instance, GRInputManager.Mode _mode)
    {
        if (__instance.currentMap == null || __instance.currentMap.Count == 0)
        {
            return;
        }

        foreach (InputActionMap actionMap in __instance.currentMap)
        {
            if (actionMap == null || (actionMap.name != "KeybordBase"))
            {
                continue;
            }

            var actionL1 = actionMap.FindAction("L1");
            if (actionL1 != null && actionL1.bindings.Count == 0)
            {
                actionL1.AddBinding("<Keyboard>/q");
                Plugin.Log.LogInfo("Binding for L1 added on the keyboard.");
            }

            var actionR1 = actionMap.FindAction("R1");
            if (actionR1 != null && actionR1.bindings.Count == 0)
            {
                actionR1.AddBinding("<Keyboard>/e");
                Plugin.Log.LogInfo("Binding for R1 added on the keyboard.");
            }
        }
    }
}
