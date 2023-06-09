using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;
using System;

public class scrInputManager : MonoBehaviour
{
    public static PlayerInputAction playerActions;

    public static event Action rebindComplete;
    public static event Action rebindCanceled;
    public static event Action<InputAction, int> rebindStarted;

    private void Awake()
    {
        if (playerActions == null) playerActions = new PlayerInputAction();
        DontDestroyOnLoad(gameObject);
    }

    public static void StartRebind(string actionName, int bindingIndex, TextMeshProUGUI statusText, bool excludeMouse)
    {
        InputAction action = playerActions.asset.FindAction(actionName);

        if (action == null)
        {
            Debug.LogError("scrInputManager->StartRebind cant find action. action is null");
            return;
        }
        if (action.bindings.Count <= bindingIndex)
        {
            Debug.LogError("scrInputManager->StartRebind bindingIndex:" + bindingIndex + " is <= action.bindings.Count: " + action.bindings.Count);
            return;
        }

        if (action.bindings[bindingIndex].isComposite == true)
        {
            var firstPartIndex = bindingIndex + 1;
            if (firstPartIndex < action.bindings.Count)
            {
                if (action.bindings[firstPartIndex].isComposite == true)
                {
                    print("firstPartIndex:" + firstPartIndex);
                    DoRebind(action, bindingIndex, statusText, true, excludeMouse);
                }
                else
                {
                    Debug.LogError("scrInputManager->StartRebind action.bindings[" + firstPartIndex + "]isComposite is false ");
                }
            }
            else
            {
                Debug.LogError("scrInputManager->StartRebind firstPartIndex: " + firstPartIndex + " is more than action.bindings.Count:  " + action.bindings.Count);
                return;
            }
        }
        else
        {
            print("bindingIndex:" + bindingIndex);
            DoRebind(action, bindingIndex, statusText, false, excludeMouse);
        }
    }

    private static void DoRebind(InputAction actionToRebind, int bindingIndex, TextMeshProUGUI statusText, bool allCompositeParts, bool excludeMouse)
    {
        if (actionToRebind == null || bindingIndex < 0) return;

        statusText.text = $"Press a {actionToRebind.expectedControlType}";

        actionToRebind.Disable();

        var rebind = actionToRebind.PerformInteractiveRebinding(bindingIndex);

        rebind.OnComplete(operation =>
        {
            actionToRebind.Enable();
            operation.Dispose();

            if (allCompositeParts == true)
            {
                var nextBindingIndex = bindingIndex + 1;
                if (nextBindingIndex < actionToRebind.bindings.Count)
                {
                    if (actionToRebind.bindings[nextBindingIndex].isComposite == true)
                    {
                        DoRebind(actionToRebind, nextBindingIndex, statusText, allCompositeParts, excludeMouse);
                    }
                }
            }

            SaveBindingOverride(actionToRebind);
            rebindComplete?.Invoke();

        });

        rebind.OnCancel(operation =>
        {
            actionToRebind.Enable();
            operation.Dispose();

            rebindCanceled?.Invoke();
        });


        rebind.WithCancelingThrough("<Keyboard>/escape");

        if (excludeMouse == true)
        {
            rebind.WithCancelingThrough("Mouse");
        }

        rebindStarted?.Invoke(actionToRebind, bindingIndex);
        rebind.Start(); // Start rebinding process
    }


    public static string GetBindingName(string actionName, int bindingIndex)
    {
        if (playerActions == null) playerActions = new PlayerInputAction();

        InputAction action = playerActions.asset.FindAction(actionName);
        return action.GetBindingDisplayString(bindingIndex);
    }

    private static void SaveBindingOverride(InputAction action)
    {
        for (int i = 0; i < action.bindings.Count; i++)
        {
            PlayerPrefs.SetString(action.actionMap + action.name + i, action.bindings[i].overridePath);
        }
    }

    public static void LoadBindingOverride(string actionName)
    {
        if (playerActions == null) playerActions = new PlayerInputAction();

        InputAction action = playerActions.asset.FindAction(actionName);

        for (int i = 0; i < action.bindings.Count; i++)
        {
            if (!string.IsNullOrEmpty(PlayerPrefs.GetString(action.actionMap + action.name + i)))
            {
                action.ApplyBindingOverride(i, PlayerPrefs.GetString(action.actionMap + action.name + i));
            }
        }

    }

    public static void ResetBinding(string actionName, int bindingIndex)
    {
        InputAction action = playerActions.asset.FindAction(actionName);
        if (action == null || action.bindings.Count <= bindingIndex)
        {
            Debug.LogError("scrInputManager->ResetBinding Could not find action or binding");
            return;
        }

        if (action.bindings[bindingIndex].isComposite)
        {
            for (int i = bindingIndex; i < action.bindings.Count && action.bindings[i].isComposite == true; i++)
            {
                action.RemoveBindingOverride(i);
            }
        }
        else
        {
            action.RemoveBindingOverride(bindingIndex);
        }

        SaveBindingOverride(action);
    }


}
