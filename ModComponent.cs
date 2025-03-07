using System;
using Il2CppInterop.Runtime.Injection;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Suikoden_Fix;

public sealed class ModComponent : MonoBehaviour
{
    public static ModComponent Instance { get; private set; }
    private bool _isDisabled;


    public bool InvertDash = false;
    public bool LastDash = false;

    public uint LastPadData = 0;
    public uint LastPadDataSanitized = 0;

    public Patches.TransitionState transition = Patches.TransitionState.None;

    public ModComponent(IntPtr ptr) : base(ptr) { }

    public static bool Inject()
    {
        ClassInjector.RegisterTypeInIl2Cpp<ModComponent>();
        var name = typeof(ModComponent).FullName;

        Plugin.Log.LogInfo($"Initializing game object {name}");
        var modObject = new GameObject(name)
        {
            hideFlags = HideFlags.HideAndDontSave
        };
        GameObject.DontDestroyOnLoad(modObject);

        Plugin.Log.LogInfo($"Adding {name} to game object...");
        ModComponent component = modObject.AddComponent<ModComponent>();
        if (component == null)
        {
            GameObject.Destroy(modObject);
            Plugin.Log.LogError($"The game object is missing the required component: {name}");
            return false;
        }

        return true;
    }

    public void Awake()
    {
        try
        {
            Instance = this;

            Plugin.Log.LogInfo($"[{nameof(ModComponent)}].{nameof(Awake)}: Processed successfully.");
        }
        catch (Exception e)
        {
            _isDisabled = true;
            Plugin.Log.LogError($"[{nameof(ModComponent)}].{nameof(Awake)}(): {e}");
        }
    }

    public void LateUpdate()
    {
        try
        {
            if (_isDisabled)
            {
                return;
            }
        }
        catch (Exception e)
        {
            _isDisabled = true;
            Plugin.Log.LogError($"[{nameof(ModComponent)}].{nameof(LateUpdate)}(): {e}");
        }
    }
}
