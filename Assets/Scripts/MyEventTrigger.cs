using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class MyEventTrigger : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private float movementValue;
    private bool _isPressed;

    public void OnPointerDown(PointerEventData eventData)
    {
        _isPressed = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        _isPressed = false;
    }

    private void Update()
    {
        if (_isPressed)
        {
            InputManager.eventMovement.Invoke(movementValue);
        }
    }
}
