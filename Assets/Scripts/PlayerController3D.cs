using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController3D : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private float gravity = -15f;
    [SerializeField] private float jumpHeight = 1.5f;

    [Header("Look / Camera")]
    [SerializeField] private Transform playerCamera;
    [Tooltip("Mouse sensitivity multiplier. Typically between 50 and 200.")]
    [SerializeField] private float mouseSensitivity = 120f;

    private CharacterController controller;
    private Vector3 velocity;
    private float xRotation = 0f;

    void Start()
    {
        controller = GetComponent<CharacterController>();

        // Auto-detect camera if not assigned in Inspector
        if (playerCamera == null)
        {
            Camera cam = GetComponentInChildren<Camera>();
            if (cam != null)
            {
                playerCamera = cam.transform;
            }
            else if (Camera.main != null)
            {
                playerCamera = Camera.main.transform;
            }
        }

        // Lock cursor to the center of the screen and hide it
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        HandleCursorLockToggle();
        HandleLook();
        HandleMovement();
    }

    private void HandleCursorLockToggle()
    {
        // Press Escape to release the cursor (convenient for Editor testing)
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        // Click back into the window to lock again
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame && Cursor.lockState != CursorLockMode.Locked)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    private void HandleLook()
    {
        if (playerCamera == null) return;

        float lookX = 0f;
        float lookY = 0f;

        // 1. Mouse delta input (new Input System)
        if (Mouse.current != null && Cursor.lockState == CursorLockMode.Locked)
        {
            Vector2 mouseDelta = Mouse.current.delta.ReadValue();
            lookX += mouseDelta.x * (mouseSensitivity * 0.001f);
            lookY += mouseDelta.y * (mouseSensitivity * 0.001f);
        }

        // 2. Gamepad right-stick input
        if (Gamepad.current != null)
        {
            Vector2 stick = Gamepad.current.rightStick.ReadValue();
            if (stick.sqrMagnitude > 0.01f)
            {
                lookX += stick.x * mouseSensitivity * Time.deltaTime;
                lookY += stick.y * mouseSensitivity * Time.deltaTime;
            }
        }

        // Up/Down: Rotate only the camera pitch and clamp to prevent neck-snapping
        xRotation -= lookY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        playerCamera.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        // Left/Right: Rotate the entire player body
        transform.Rotate(Vector3.up * lookX);
    }

    private void HandleMovement()
    {
        // Grounded check stabilization
        if (controller.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        Vector2 input = Vector2.zero;

        // Keyboard (WASD & Arrow keys)
        if (Keyboard.current != null)
        {
            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) input.y += 1f;
            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) input.y -= 1f;
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) input.x -= 1f;
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) input.x += 1f;
        }

        // Gamepad Left Stick
        if (Gamepad.current != null)
        {
            Vector2 stick = Gamepad.current.leftStick.ReadValue();
            if (stick.sqrMagnitude > 0.04f)
            {
                input += stick;
            }
        }

        // Clamp diagonal movement so diagonal isn't faster
        input = Vector2.ClampMagnitude(input, 1f);

        // Move relative to player facing direction
        Vector3 move = transform.right * input.x + transform.forward * input.y;
        controller.Move(move * moveSpeed * Time.deltaTime);

        // Jump logic
        bool jumpPressed = false;
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            jumpPressed = true;
        }
        if (Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame)
        {
            jumpPressed = true;
        }

        if (jumpPressed && controller.isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        // Apply gravity
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
}