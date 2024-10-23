using System;

#if ENABLE_INPUT_SYSTEM

using UnityEngine;
using UnityEngine.InputSystem;

public static partial class UtilityMethods
{

    public static string InterfaceToStringName(Type type, string replaced = null, string replacedwith = "")
    {
        string Name = "";
        if (replaced != null)
            Name = type.Name.Replace(replaced, replacedwith);
        return Name.Substring(1);
    }

    public static void BindPlayerAction<T>(T playerAction, InputActionMap map)
        where T : IControlBinder
    {
        System.Type type = playerAction.InputInterface;
        foreach (var method in type.GetMethods())
        {
            string actionName = method.Name.Substring(2);
            InputAction action = map.FindAction(actionName);

            if (action != null)
            {
                var delegates =
                    (Action<InputAction.CallbackContext>)method.CreateDelegate(typeof(Action<InputAction.CallbackContext>), playerAction);
                action.Reset();
                action.started += delegates;
                action.performed += delegates;
                action.canceled += delegates;
                Debug.Log($"Bind {actionName} to {action}");
            }
            else
                Debug.Log($"Unable to find {actionName} in {map}");
        }
    }

    public static ControlSchemeEnum GetDeviceType(InputDevice device)
    {
        ControlSchemeEnum scheme = ControlSchemeEnum.Gamepad;

        if(device is Gamepad)
            scheme = ControlSchemeEnum.Gamepad;
        else if(device is Keyboard)
            scheme = ControlSchemeEnum.Keyboard;

        return scheme;
    }

    public static string GetControlType(this ControlSchemeEnum control)
    {
        string mode = null;
        switch (control)
        {
            case ControlSchemeEnum.Keyboard:
                mode = nameof(Keyboard);
                break;
            case ControlSchemeEnum.Gamepad:
                mode = nameof(Gamepad);
                break;
        }
        return mode;
    }
}

public enum ControlSchemeEnum
{
    Keyboard,
    Gamepad
}

#endif