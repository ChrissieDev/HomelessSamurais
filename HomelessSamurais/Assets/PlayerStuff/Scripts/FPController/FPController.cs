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
    
    // Sync full position including Y
    private NetworkVariable<Vector3> networkPosition = new NetworkVariable<Vector3>(
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
            // Initialize network position immediately
            networkPosition.Value = transform.position;
            networkRotationY.Value = transform.rotation.eulerAngles.y;
            
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
            // Set initial position from network variables
            if (networkPosition.Value != Vector3.zero)
            {
                transform.position = networkPosition.Value;
            }
            
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
        if (!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer)
        {
            return;
        }
        
        if (IsOwner)
        {
            // Owner handles input and movement
            HandleMovement();
            HandleCameraRotation();
            
            // Update network variables with full position
            networkPosition.Value = transform.position;
            networkRotationY.Value = transform.rotation.eulerAngles.y;
        }
        else
        {
            // Non-owners smoothly interpolate to synced position
            transform.position = Vector3.Lerp(transform.position, networkPosition.Value, Time.deltaTime * 10f);
            
            Quaternion targetRotation = Quaternion.Euler(0, networkRotationY.Value, 0);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
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
                velocity.y = -2f;
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