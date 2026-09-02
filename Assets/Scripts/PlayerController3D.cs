using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

[RequireComponent(typeof(CharacterController))]
public class PlayerController3D : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private float gravity = -15f;

    private CharacterController controller;
    private Vector3 velocity;

    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        HandleMovement();
    }

    private void HandleMovement()
    {
        // Keep grounded check stable
        if (controller.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        // Horizontal input: A/D, Left/Right arrows (-1 to 1)
        float x = GetHorizontalInput();

        // Move left and right relative to player facing direction
        Vector3 move = transform.right * x;
        controller.Move(move * moveSpeed * Time.deltaTime);

        // Apply falling gravity to stay grounded
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    private float GetHorizontalInput()
    {
        float x = 0f;

#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null)
        {
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) x -= 1f;
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) x += 1f;
        }

        if (Gamepad.current != null && Mathf.Approximately(x, 0f))
        {
            x = Gamepad.current.leftStick.x.ReadValue();
        }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER || (!ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER)
        if (Mathf.Approximately(x, 0f))
        {
            x = Input.GetAxisRaw("Horizontal");
        }
#endif

        return x;
    }
}