using HarmonyLib;
using UnityEngine.InputSystem;

namespace Suikoden_Fix.Patches;

public class RemoveBindingPatch
{
    [HarmonyPatch(typeof(GRInputManager), nameof(GRInputManager.Create))]
    [HarmonyPostfix]
    static void Mapping(GRInputManager __instance, GRInputManager.Mode _mode)
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
}
