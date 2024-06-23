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

    public Vector2 input = Vector2.zero;
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

    // wallrunning
    float currentMaxVelocity;
    public bool isWallruning = false;
    RaycastHit wallrunWall;

    // sliding
    public bool isSliding = false;
    
    
    void Start()
    {

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        currentMaxVelocity = playerSettings.maxGroundVelocity;
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
        if (isGrounded || isWallruning)
            coyoteTimer = playerSettings.coyoteTime;
    }

    void ProcessMovement()
    {
        ApplyDrag();

        if(isWallruning)
            ApplyWallrunForce();
        else
            ApplyMovementForce();

        if(CanWallrun())
            EnterWallrun();
        else
            ExitWallrun();

        ProcessJump();

        LimitHorizontalVelocity();
    }

    void ApplyDrag()
    {
        if(isGrounded)
        {
            _rb.drag = playerSettings.groundDrag;
            return;
        }

        if(isSliding)
        {
            _rb.drag = playerSettings.slideDrag;
            return;
        }

        _rb.drag = playerSettings.airDrag;
    }

    void ApplyMovementForce()
    {
        Vector3 wishDir = orientation.forward * input.y + orientation.right * input.x;
        Vector3 horizontalAcceleration = wishDir.normalized * playerSettings.maxAcceleration * (isGrounded ? 1 : playerSettings.airControlAuthority);
        _rb.AddForce(horizontalAcceleration, ForceMode.Acceleration);
    }


    void ApplyWallrunForce()
    {
        _rb.velocity = new(_rb.velocity.x, _rb.velocity.y * Time.fixedDeltaTime * playerSettings.wallrunVerticalDampingFactor, _rb.velocity.z);

        Vector3 wallForward = Vector3.Cross(wallrunWall.normal, Vector3.up);
        if((orientation.forward - wallForward).magnitude > (orientation.forward + wallForward).magnitude)
            wallForward = -wallForward;

        Vector3 wallrunAcceleration = wallForward * input.y * playerSettings.maxAcceleration;
        _rb.AddForce(wallrunAcceleration, ForceMode.Acceleration);
    }

    void LimitHorizontalVelocity()
    {
        currentMaxVelocity = (isGrounded && !isSliding) ? playerSettings.maxGroundVelocity : playerSettings.maxAirVelocity; 
        
        
        Vector3 flatVelocity = new(_rb.velocity.x, 0, _rb.velocity.z);
        if (flatVelocity.magnitude > currentMaxVelocity)
        {
            Vector3 lerp = Vector3.Lerp(flatVelocity, flatVelocity.normalized * currentMaxVelocity, 0.5f);
            _rb.velocity = new(lerp.x, _rb.velocity.y, lerp.z);
        }
    }

    void ProcessJump()
    {
        if(coyoteTimer <= 0f || jumpBufferTimer <= 0f)
            return;

        if(isWallruning)
        {
            WallrunJump();
            jumpBufferTimer = 0;            
            coyoteTimer = 0f;
            return;
        }
        else
        {
            _rb.AddForce(Vector3.up * playerSettings.jumpForce, ForceMode.Acceleration);
            jumpBufferTimer = 0;            
            coyoteTimer = 0f;
        }
    }

    void WallrunJump()
    {
        Vector3 jumpVector = (wallrunWall.normal + Vector3.up + orientation.forward).normalized;
        Vector3 jumpAcceleration = jumpVector * playerSettings.wallrunJumpRepelForce;
        _rb.AddForce(jumpAcceleration, ForceMode.Acceleration);
    }

    bool CanWallrun()
    {
        if (isGrounded || isSliding)
            return false;

        if(Physics.Raycast(transform.position, orientation.right, out wallrunWall, playerSettings.maxWallDistance, playerSettings.groundMask) && input.x > 0 )
            return true;

        if(Physics.Raycast(transform.position, -orientation.right, out wallrunWall, playerSettings.maxWallDistance, playerSettings.groundMask) && input.x < 0 )
            return true;

        return false;
    }

    void EnterWallrun()
    {
        _rb.useGravity = false;
        isWallruning = true;
    }    

    void ExitWallrun()
    {
        _rb.useGravity = true;
        isWallruning = false;
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

        keybinds.Movement.Slide.performed += OnSlidePerformed;
        keybinds.Movement.Slide.canceled += OnSlideCanceled;
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

        keybinds.Movement.Slide.performed -= OnSlidePerformed;
        keybinds.Movement.Slide.canceled -= OnSlideCanceled;
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
    }

    void OnJumpCanceled(InputAction.CallbackContext context)
    {

    }

    void OnSlidePerformed(InputAction.CallbackContext context)
    {
        isSliding = true;
    }
    void OnSlideCanceled(InputAction.CallbackContext context)
    {
        isSliding = false;        
    }



    // --- getters ---
    public float GetFlatVelocity()
    {
        return new Vector3(_rb.velocity.x, 0, _rb.velocity.z).magnitude;
    }

}
