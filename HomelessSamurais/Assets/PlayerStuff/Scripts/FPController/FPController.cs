using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPController : MonoBehaviour
{
    public float WalkSpeed = 5f;
    public float SprintMultiplier = 1.5f;
    public float JumpForce = 5f;
    public float GroundCheckDistance = 0.4f;
    public float LookSensitivityX = 1f;
    public float LookSensitivityY = 1f;
    public float MinYLookAngle = -90f;
    public float MaxYLookAngle = 90f;
    public Transform FPCamera;
    public float Gravity = -9.8f;
    
    private Vector3 velocity;
    private float verticalRotation = 0f;
    private CharacterController characterController;
    private Dashing dashing;
    private bool isLocalPlayer = true;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        dashing = GetComponent<Dashing>();
        
        // locks cursor for local player
        if (isLocalPlayer)
        {
            Cursor.lockState = CursorLockMode.Locked;
        }
        
        // this disables the camera if ur not a local player
        if (FPCamera != null && !isLocalPlayer)
        {
            Camera cam = FPCamera.GetComponent<Camera>();
            if (cam != null) cam.enabled = false;
            
            AudioListener listener = FPCamera.GetComponent<AudioListener>();
            if (listener != null) listener.enabled = false;
        }
    }

    void Update()
    {
        // process input for local player
        if (!isLocalPlayer) return;

        float horizontalMovement = Input.GetAxisRaw("Horizontal");
        float verticalMovement = Input.GetAxisRaw("Vertical");

        Vector3 moveDirection = transform.forward * verticalMovement + transform.right * horizontalMovement;
        moveDirection.Normalize();

        float speed = WalkSpeed;
        if(Input.GetAxis("Sprint") > 0)
        {
            speed *= SprintMultiplier;
        }

        // only apply normal movement if not dashing
        if (dashing == null || !dashing.IsDashing())
        {
            characterController.Move(moveDirection * speed * Time.deltaTime);
        }

        if(Input.GetButtonDown("Jump") && IsGrounded())
        {
            velocity.y = Mathf.Sqrt(JumpForce * -2f * Gravity);
        }

        velocity.y += Gravity * Time.deltaTime;
        
        // combine gravity with dash velocity (during dash or decay)
        Vector3 finalMovement = velocity * Time.deltaTime;
        if (dashing != null && (dashing.IsDashing() || dashing.IsDecaying()))
        {
            finalMovement += dashing.GetDashVelocity() * Time.deltaTime;
        }
        
        characterController.Move(finalMovement);

        HandleCameraRotation();
    }

    void HandleCameraRotation()
    {
        if(FPCamera != null)
        {
            float mouseX = Input.GetAxis("Mouse X") * LookSensitivityX;
            float mouseY = Input.GetAxis("Mouse Y") * LookSensitivityY;

            verticalRotation -= mouseY;
            verticalRotation = Mathf.Clamp(verticalRotation, MinYLookAngle, MaxYLookAngle);

            FPCamera.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);
            transform.Rotate(Vector3.up * mouseX);
        }
    }

    bool IsGrounded()
    {
        if (Physics.Raycast(transform.position, Vector3.down, GroundCheckDistance + 0.1f))
        {
            return true;
        }
        return false;
    }
    
    // public method to reset vertical velocity (called by dashing)
    public void ResetVerticalVelocity()
    {
        velocity.y = 0f;
    }
    
    public void SetLocalPlayer(bool isLocal)
    {
        isLocalPlayer = isLocal;
        
        if (dashing != null)
        {
            dashing.SetLocalPlayer(isLocal);
        }
        
        if (isLocal)
        {
            Cursor.lockState = CursorLockMode.Locked;
        }
    }
}