﻿using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.UI;
using UnityEngine.InputSystem.Users;
#endif

#if ENABLE_INPUT_SYSTEM
[RequireComponent(typeof(PlayerInput))]
#endif
public class GamepadCursor : MonoBehaviour
{

    [SerializeField]
    private RectTransform cursorTransform;
    [SerializeField]
    private RectTransform canvasRectransform;
    [SerializeField]
    private Canvas canvas;
    [SerializeField]
    private Camera mainCamera;
    [SerializeField]
    private float cursorSpeed = 1000;

    private void Awake()
    {
        mainCamera = Camera.main;
#if ENABLE_INPUT_SYSTEM
        PlayerInput sc = gameObject.AddComponent(typeof(PlayerInput)) as PlayerInput;
        playerInput = sc;

        playerInput.actions = (Resources.Load<InputActionAsset>("ScutiGamepad"));
        playerInput.camera = mainCamera;
        playerInput.uiInputModule = EventSystem.current.GetComponent<InputSystemUIInputModule>();
#endif
    }


#if ENABLE_INPUT_SYSTEM

    [SerializeField]
    private PlayerInput playerInput;

    private bool previousMouseState;

    private Mouse virtualMouse;

    private void OnEnable()
    {
        if (virtualMouse == null)
        {
            virtualMouse = (Mouse)InputSystem.AddDevice("VirtualMouse");
        }
        else if (!virtualMouse.added)
        {
            InputSystem.AddDevice(virtualMouse);
        }

        // Pair the device to the user to use the playerInput component with the EventSystem & virtual mouse
        InputUser.PerformPairingWithDevice(virtualMouse, playerInput.user);

        if (cursorTransform != null)
        {
            Vector2 position = cursorTransform.anchoredPosition;
            InputState.Change(virtualMouse.position, position);
        }

        InputSystem.onAfterUpdate += UpdateMotion;
        //playerInput.onControlsChanged += OnControlsChanged;

    }

    private void OnDisable()
    {
        InputSystem.RemoveDevice(virtualMouse);
        InputSystem.onAfterUpdate -= UpdateMotion;
        //playerInput.onControlsChanged -= OnControlsChanged;
    }

    private void UpdateMotion()
    {
        if (virtualMouse == null || Gamepad.current == null)
        {
            return;
        }

        Vector2 deltaValue = Gamepad.current.leftStick.ReadValue();
        deltaValue *= cursorSpeed * Time.deltaTime;

        Vector2 currentPosition = virtualMouse.position.ReadValue();
        Vector2 newPosition = currentPosition + deltaValue;

        newPosition.x = Mathf.Clamp(newPosition.x, 0, Screen.width); // TODO -- add padding
        newPosition.y = Mathf.Clamp(newPosition.y, 0, Screen.height);

        InputState.Change(virtualMouse.position, newPosition);
        InputState.Change(virtualMouse.delta, deltaValue);

        bool aButtonIsPressed = Gamepad.current.aButton.IsPressed();
        if (previousMouseState != Gamepad.current.aButton.isPressed)
        {
            virtualMouse.CopyState<MouseState>(out var mouseState);
            mouseState.WithButton(MouseButton.Left, aButtonIsPressed);
            InputState.Change(virtualMouse, mouseState);
            previousMouseState = aButtonIsPressed;
        }

        AnchorCursor(newPosition);

    }

    private void AnchorCursor(Vector2 position)
    {
        Vector2 anchoredPosition;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRectransform, position, canvas.renderMode
            == RenderMode.ScreenSpaceOverlay ? null : mainCamera, out anchoredPosition);


        cursorTransform.anchoredPosition = anchoredPosition;

    }
#endif
}

