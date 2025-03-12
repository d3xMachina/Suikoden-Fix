using System.Collections.Generic;
using System.Linq;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem;

namespace Suikoden_Fix.Tools.Input;

public class Command
{
    public bool IsOn { get; private set; }

    public Command(List<GamepadButton> buttons, List<Key> keys, List<GRInputManager.Type> gameInput, bool isCombination = false)
    {
        _buttons = buttons;
        _keys = keys;
        _gameInputs = gameInput;
        _isCombination = isCombination;
        _wasPressed = false;
    }

    public void Update()
    {
        bool pressed;

        if (_isCombination)
        {
            pressed = CheckKeyCombination() || CheckButtonCombination() || CheckGameInputCombination();
        }
        else
        {
            pressed = CheckAnyKeyPressed() || CheckAnyButtonPressed() || CheckAnyGameInputPressed();
        }

        IsOn = pressed && !_wasPressed;
        _wasPressed = pressed;
    }

    private bool CheckKeyCombination()
    {
        return _keys.Count > 0 && _keys.All(GRInputManager.IsKeyPress);
    }

    private bool CheckButtonCombination()
    {
        var gamepad = Gamepad.current;
        return gamepad != null && _buttons.Count > 0 && _buttons.All(button => gamepad[button].isPressed);
    }

    private bool CheckGameInputCombination()
    {
        return _gameInputs.Count > 0 && _gameInputs.All(GRInputManager.IsPress);
    }

    private bool CheckAnyKeyPressed()
    {
        return _keys.Any(GRInputManager.IsKeyPress);
    }

    private bool CheckAnyButtonPressed()
    {
        var gamepad = Gamepad.current;
        return gamepad != null && _buttons.Any(button => gamepad[button].isPressed);
    }

    private bool CheckAnyGameInputPressed()
    {
        return _gameInputs.Any(GRInputManager.IsPress);
    }

    private List<Key> _keys;
    private List<GamepadButton> _buttons;
    private List<GRInputManager.Type> _gameInputs;
    private bool _isCombination;
    private bool _wasPressed;
}
