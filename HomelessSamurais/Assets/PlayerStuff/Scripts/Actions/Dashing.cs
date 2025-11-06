using System.Collections;
using UnityEngine;

public class Dashing : MonoBehaviour
{
    [Header("Dash Settings")]
    public float dashSpeed = 100f;
    public float dashDuration = 0.12f;
    public float dashDecayDuration = 0.2f; // how long to fade out after dash
    public float dashCooldown = 1f;
    
    [Header("Camera Follow Settings")]
    [Range(0f, 1f)]
    public float cameraInfluence = 1f; // 0 = locked direction, 1 = full curve following camera
    public bool followVerticalLook = true; // whether to dash in 3d space (up/down) or just horizontal
    
    [Header("References")]
    public Transform playerCamera;
    
    private CharacterController characterController;
    private bool isDashing = false;
    private bool isDecaying = false; // separate flag for decay phase
    private bool canDash = true;
    private bool isLocalPlayer = true;
    private FPController fpController;
    private Vector3 dashVelocity; // store dash velocity
    
    void Start()
    {
        characterController = GetComponent<CharacterController>();
        fpController = GetComponent<FPController>();
        
        // if camera not assigned, try to find child camera
        if (playerCamera == null)
        {
            playerCamera = GetComponentInChildren<Camera>()?.transform;
        }
    }
    
    void Update()
    {
        // only process input for local player
        if (!isLocalPlayer) return;
        
        // use the "Dash" axis from input manager
        if (Input.GetButtonDown("Dash") && canDash && !isDashing && !isDecaying)
        {
            StartCoroutine(PerformDash());
        }
    }
    
    IEnumerator PerformDash()
    {
        isDashing = true;
        canDash = false;
        
        // get initial movement input direction
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        
        // store initial direction for blending
        Vector3 initialDirection;
        
        // if no input, dash in the direction player is facing (camera forward)
        if (horizontal == 0 && vertical == 0)
        {
            if (followVerticalLook)
            {
                // dash in full 3d camera direction
                initialDirection = playerCamera.forward.normalized;
            }
            else
            {
                // dash only horizontally
                initialDirection = new Vector3(playerCamera.forward.x, 0f, playerCamera.forward.z).normalized;
            }
        }
        else
        {
            // get camera-relative direction
            Vector3 cameraForward;
            Vector3 cameraRight;
            
            if (followVerticalLook)
            {
                // use full 3d camera vectors
                cameraForward = playerCamera.forward.normalized;
                cameraRight = playerCamera.right.normalized;
            }
            else
            {
                // flatten to horizontal plane
                cameraForward = new Vector3(playerCamera.forward.x, 0f, playerCamera.forward.z).normalized;
                cameraRight = new Vector3(playerCamera.right.x, 0f, playerCamera.right.z).normalized;
            }
            
            // calculate direction relative to camera
            initialDirection = (cameraRight * horizontal + cameraForward * vertical).normalized;
        }
        
        float timer = 0f;
        
        // dash at full speed
        while (timer < dashDuration)
        {
            Vector3 moveDirection;
            
            if (cameraInfluence > 0f)
            {
                // recalculate camera-relative direction every frame for curving
                Vector3 cameraForward;
                Vector3 cameraRight;
                
                if (followVerticalLook)
                {
                    // use full 3d camera vectors
                    cameraForward = playerCamera.forward.normalized;
                    cameraRight = playerCamera.right.normalized;
                }
                else
                {
                    // flatten to horizontal plane
                    cameraForward = new Vector3(playerCamera.forward.x, 0f, playerCamera.forward.z).normalized;
                    cameraRight = new Vector3(playerCamera.right.x, 0f, playerCamera.right.z).normalized;
                }
                
                Vector3 currentDirection = (cameraRight * horizontal + cameraForward * vertical).normalized;
                
                // if no input, use camera forward
                if (horizontal == 0 && vertical == 0)
                {
                    if (followVerticalLook)
                    {
                        currentDirection = playerCamera.forward.normalized;
                    }
                    else
                    {
                        currentDirection = new Vector3(playerCamera.forward.x, 0f, playerCamera.forward.z).normalized;
                    }
                }
                
                // blend between initial direction and current camera direction
                moveDirection = Vector3.Lerp(initialDirection, currentDirection, cameraInfluence).normalized;
            }
            else
            {
                // no camera influence, use initial direction
                moveDirection = initialDirection;
            }
            
            // full speed dash
            dashVelocity = moveDirection * dashSpeed;
            
            timer += Time.deltaTime;
            yield return null;
        }
        
        isDashing = false; // dash main phase over
        isDecaying = true; // now in decay phase
        
        // gradually fade out dash velocity after dash ends
        float decayTimer = 0f;
        Vector3 finalDashVelocity = dashVelocity;
        
        while (decayTimer < dashDecayDuration)
        {
            decayTimer += Time.deltaTime;
            float decayAmount = 1f - (decayTimer / dashDecayDuration);
            dashVelocity = finalDashVelocity * decayAmount;
            yield return null;
        }
        
        dashVelocity = Vector3.zero;
        isDecaying = false;

        // cooldown
        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }
    
    public bool IsDashing()
    {
        return isDashing; // only returns true during main dash, not decay
    }
    
    public bool IsDecaying()
    {
        return isDecaying;
    }
    
    public void SetLocalPlayer(bool isLocal)
    {
        isLocalPlayer = isLocal;
    }
    
    public Vector3 GetDashVelocity()
    {
        return dashVelocity;
    }
}