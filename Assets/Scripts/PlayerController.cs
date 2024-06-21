using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;


[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] PlayerSettings playerSettings;

    Keybinds keybinds;

    Vector2 input = Vector2.zero;
    Rigidbody _rb;

    [SerializeField] ObjectManipulator objectManipulator;
    [SerializeField] Transform cam;
    [SerializeField] Transform orientation;
    float camElevation = 0;
    float camHeading = 0;


    // jumping
    public bool isGrounded = false;

    double coyoteTimer = 0;
    double jumpBufferTimer = 0;
    bool jumpButtonDown = false;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        UpdateJumpTimers();
        cam.position = this.transform.position + Vector3.up;
    }
    void Awake()
    {
        keybinds = new();
        _rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        ProcessGround();
        ProcessMovement();
    }

    void UpdateJumpTimers()
    {
        coyoteTimer -= Time.deltaTime;
        jumpBufferTimer -= Time.deltaTime;
    }


    bool CheckGround()
    {
        if (!Physics.Raycast(this.transform.position, Vector3.down, out RaycastHit hit, Mathf.Infinity, playerSettings.groundMask))
            return false;

        return Vector3.Distance(hit.point, this.transform.position) <= 1.01f;
    }

    void ProcessGround()
    {
        isGrounded = CheckGround();
        if (isGrounded)
            coyoteTimer = playerSettings.coyoteTime;
    }

    void ProcessMovement()
    {
        _rb.drag = isGrounded ? playerSettings.walkDrag : playerSettings.fallDrag;
        Vector3 wishDir = orientation.forward * input.y + orientation.right * input.x;
        Vector3 horizontalAcceleration = wishDir.normalized * playerSettings.maxAcceleration * (isGrounded ? 1 : playerSettings.airControlAuthority);
        _rb.AddForce(horizontalAcceleration, ForceMode.Acceleration);

        LimitHorizontalVelocity();
    }

    void LimitHorizontalVelocity()
    {
        Vector3 flatVelocity = new(_rb.velocity.x, 0, _rb.velocity.z);
        if (flatVelocity.magnitude > playerSettings.maxHorizontalSpeed)
        {
            flatVelocity = flatVelocity.normalized * playerSettings.maxHorizontalSpeed;
            _rb.velocity = new(flatVelocity.x, _rb.velocity.y, flatVelocity.z);
        }
        ProcessJump();
    }

    void ProcessJump()
    {
        if(coyoteTimer > 0f && jumpBufferTimer > 0f)
        {
            _rb.AddForce(Vector3.up * playerSettings.jumpForce, ForceMode.Acceleration);
            jumpBufferTimer = 0;            
            coyoteTimer = 0f;

        }
    }

    // --- key processing ---
    void OnEnable()
    {
        keybinds.Enable();
        keybinds.Movement.Movement.performed += OnMovementPerformed;
        keybinds.Movement.Movement.canceled += OnMovementCanceled;

        keybinds.Movement.Look.performed += OnLookPerformed;

        keybinds.Movement.Grab.performed += OnGrabPerformed;
        keybinds.Movement.Grab.canceled += OnGrabCanceled;

        keybinds.Movement.Jump.performed += OnJumpPerformed;
        keybinds.Movement.Jump.canceled += OnJumpCanceled;
    }
    void OnDisable()
    {
        keybinds.Disable();
        keybinds.Movement.Movement.performed -= OnMovementPerformed;
        keybinds.Movement.Movement.canceled -= OnMovementCanceled;

        keybinds.Movement.Look.performed -= OnLookPerformed;

        keybinds.Movement.Grab.performed -= OnGrabPerformed;
        keybinds.Movement.Grab.canceled -= OnGrabCanceled;

        keybinds.Movement.Jump.performed -= OnJumpPerformed;
        keybinds.Movement.Jump.canceled -= OnJumpCanceled;
    }
    void OnMovementCanceled(InputAction.CallbackContext context)
    {
        input = Vector2.zero;
    }

    void OnMovementPerformed(InputAction.CallbackContext context)
    {
        input = context.ReadValue<Vector2>();
    }

    void OnLookPerformed(InputAction.CallbackContext context)
    {
        Vector2 mouseDelta = context.ReadValue<Vector2>();
        camHeading += mouseDelta.x * playerSettings.cameraSensitivity;

        orientation.rotation = Quaternion.Euler(new Vector3(0, camHeading, 0));

        camElevation = Mathf.Clamp(camElevation - mouseDelta.y * playerSettings.cameraSensitivity, -90, 90);
        cam.localRotation = Quaternion.Euler(new Vector3(camElevation, camHeading, 0));
    }

    void OnGrabPerformed(InputAction.CallbackContext context)
    {
        objectManipulator.TryGrab();
    }
    void OnGrabCanceled(InputAction.CallbackContext context)
    {
        objectManipulator.Drop();
    }

    void OnJumpPerformed(InputAction.CallbackContext context)
    {
        jumpBufferTimer = playerSettings.jumpInputBufferTime;
        jumpButtonDown = true;
    }

    void OnJumpCanceled(InputAction.CallbackContext context)
    {
        jumpButtonDown = false;
    }
}
