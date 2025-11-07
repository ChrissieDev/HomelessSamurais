using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class FPController : NetworkBehaviour
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
    
    // Only sync horizontal position (X and Z)
    private NetworkVariable<Vector2> networkPositionXZ = new NetworkVariable<Vector2>(
        default, 
        NetworkVariableReadPermission.Everyone, 
        NetworkVariableWritePermission.Owner);
    
    private NetworkVariable<float> networkRotationY = new NetworkVariable<float>(
        default, 
        NetworkVariableReadPermission.Everyone, 
        NetworkVariableWritePermission.Owner);

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        dashing = GetComponent<Dashing>();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        
        if (IsOwner)
        {
            Cursor.lockState = CursorLockMode.Locked;
            
            if (FPCamera != null)
            {
                Camera cam = FPCamera.GetComponent<Camera>();
                if (cam != null) cam.enabled = true;
                
                AudioListener listener = FPCamera.GetComponent<AudioListener>();
                if (listener != null) listener.enabled = true;
            }
            
            if (dashing != null)
            {
                dashing.SetLocalPlayer(true);
            }
        }
        else
        {
            if (FPCamera != null)
            {
                Camera cam = FPCamera.GetComponent<Camera>();
                if (cam != null) cam.enabled = false;
                
                AudioListener listener = FPCamera.GetComponent<AudioListener>();
                if (listener != null) listener.enabled = false;
            }
            
            if (dashing != null)
            {
                dashing.SetLocalPlayer(false);
            }
            
            // Disable CharacterController for remote players
            if (characterController != null)
            {
                characterController.enabled = false;
            }
        }
    }

    void Update()
    {
        // stops processing movement if not connected to network yet
        if (!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer)
        {
            return;
        }
        
        if (IsOwner)
        {
            // Owner handles input and movement
            HandleMovement();
            HandleCameraRotation();
            
            // Update network variables - only horizontal position
            networkPositionXZ.Value = new Vector2(transform.position.x, transform.position.z);
            networkRotationY.Value = transform.rotation.eulerAngles.y;
        }
        else
        {
            // Non-owners interpolate to synced horizontal position only
            Vector3 targetPos = new Vector3(
                networkPositionXZ.Value.x, 
                transform.position.y,  // Keep current Y (let gravity work locally)
                networkPositionXZ.Value.y
            );
            
            transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * 15f);
            
            Quaternion targetRotation = Quaternion.Euler(0, networkRotationY.Value, 0);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * 15f);
            
            // Apply gravity for remote players too
            if (!characterController.isGrounded)
            {
                velocity.y += Gravity * Time.deltaTime;
                Vector3 verticalMovement = new Vector3(0, velocity.y, 0) * Time.deltaTime;
                transform.position += verticalMovement;
            }
            else
            {
                velocity.y = -2f;
            }
        }
    }
    
    void HandleMovement()
    {
        float horizontalMovement = Input.GetAxisRaw("Horizontal");
        float verticalMovement = Input.GetAxisRaw("Vertical");

        Vector3 moveDirection = transform.forward * verticalMovement + transform.right * horizontalMovement;
        moveDirection.Normalize();

        float speed = WalkSpeed;
        if (Input.GetKey(KeyCode.LeftShift))
        {
            speed *= SprintMultiplier;
        }

        // Handle horizontal movement
        if (dashing == null || !dashing.IsDashing())
        {
            characterController.Move(moveDirection * speed * Time.deltaTime);
        }

        // Handle vertical movement (gravity & jumping)
        if (characterController.isGrounded)
        {
            if (velocity.y < 0)
            {
                velocity.y = -2f; // Keep grounded
            }
            
            if (Input.GetButtonDown("Jump"))
            {
                velocity.y = Mathf.Sqrt(JumpForce * -2f * Gravity);
            }
        }

        velocity.y += Gravity * Time.deltaTime;
        
        // Apply vertical movement + dash
        Vector3 verticalMovement2 = new Vector3(0, velocity.y, 0) * Time.deltaTime;
        
        if (dashing != null && (dashing.IsDashing() || dashing.IsDecaying()))
        {
            Vector3 dashVel = dashing.GetDashVelocity();
            characterController.Move(dashVel * Time.deltaTime + verticalMovement2);
        }
        else
        {
            characterController.Move(verticalMovement2);
        }
    }

    void HandleCameraRotation()
    {
        if (FPCamera != null)
        {
            float mouseX = Input.GetAxis("Mouse X") * LookSensitivityX;
            float mouseY = Input.GetAxis("Mouse Y") * LookSensitivityY;

            verticalRotation -= mouseY;
            verticalRotation = Mathf.Clamp(verticalRotation, MinYLookAngle, MaxYLookAngle);

            FPCamera.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);
            transform.Rotate(Vector3.up * mouseX);
        }
    }
    
    public void ResetVerticalVelocity()
    {
        velocity.y = 0f;
    }
}