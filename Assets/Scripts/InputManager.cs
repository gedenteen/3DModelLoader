using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static UnityEvent<float> eventMovement = new UnityEvent<float>();
    public static UnityEvent<Vector2> eventRotation = new UnityEvent<Vector2>();

    private InputMap _inputMap;

    private void Awake()
    {
        _inputMap = new InputMap();
        _inputMap.Enable();
        _inputMap.TouchScreen.TouchDelta.performed += OnTouchDeltaPerformed;
    }

    private void OnTouchDeltaPerformed(InputAction.CallbackContext context)
    {
        eventRotation.Invoke(context.ReadValue<Vector2>());
    }
}
