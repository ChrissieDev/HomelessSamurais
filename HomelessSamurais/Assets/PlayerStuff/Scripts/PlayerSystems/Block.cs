using UnityEngine;
using Unity.Netcode;

public class Block : NetworkBehaviour
{
    [Header("Block Settings")]
    public float blockDamageReduction = 0.8f;
    
    [Header("Perfect Block")]
    public bool enablePerfectBlock = true;
    public float perfectBlockWindow = 0.2f;
    
    [Header("Knockback")]
    public float knockbackForce = 5f;
    public float knockbackUpwardForce = 2f;
    public float knockbackDuration = 0.2f;
    
    [Header("References")]
    public Animator combatAnimator;
    
    private NetworkVariable<bool> isBlocking = new NetworkVariable<bool>(false, 
        NetworkVariableReadPermission.Everyone, 
        NetworkVariableWritePermission.Owner);
    
    private float blockStartTime;
    private CharacterController characterController;
    private Vector3 knockbackVelocity;
    private float knockbackEndTime;
    
    void Start()
    {
        characterController = GetComponent<CharacterController>();
    }
    
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        
        // Subscribe to blocking state changes
        isBlocking.OnValueChanged += OnBlockingChanged;
    }
    
    public override void OnNetworkDespawn()
    {
        isBlocking.OnValueChanged -= OnBlockingChanged;
        base.OnNetworkDespawn();
    }
    
    void OnBlockingChanged(bool previousValue, bool newValue)
    {
        // Play block animation for all clients when blocking state changes
        if (newValue && combatAnimator != null)
        {
            combatAnimator.SetTrigger("Block");
        }
    }
    
    void Update()
    {
        // Only owner processes input
        if (!IsOwner) return;
        
        // Apply knockback (including vertical movement)
        if (Time.time < knockbackEndTime)
        {
            // Apply gravity to knockback velocity
            knockbackVelocity.y += Physics.gravity.y * Time.deltaTime;
            characterController.Move(knockbackVelocity * Time.deltaTime);
        }
        
        // Handle block input
        if (Input.GetButton("Block"))
        {
            if (!isBlocking.Value)
            {
                StartBlock();
            }
        }
        else
        {
            if (isBlocking.Value)
            {
                EndBlock();
            }
        }
    }
    
    void StartBlock()
    {
        isBlocking.Value = true;
        blockStartTime = Time.time;
    }
    
    void EndBlock()
    {
        isBlocking.Value = false;
    }
    
    public void OnBlockedAttack(float incomingDamage, Vector3 attackerPosition)
    {
        if (!isBlocking.Value) return;
        
        Debug.Log("OnBlockedAttack called. Incoming damage: " + incomingDamage);
        
        bool isPerfectBlock = enablePerfectBlock && (Time.time - blockStartTime) <= perfectBlockWindow;
        
        if (isPerfectBlock)
        {
            Debug.Log("Perfect Block!");
        }
        else
        {
            float reducedDamage = incomingDamage * (1f - blockDamageReduction);
            Debug.Log("Normal block. Reduced damage: " + reducedDamage);
            
            Health health = GetComponent<Health>();
            if (health != null)
            {
                health.TakeDamage(reducedDamage);
            }
        }
        
        // Apply knockback away from attacker with upward force
        Vector3 knockbackDirection = (transform.position - attackerPosition).normalized;
        knockbackDirection.y = 0;
        
        knockbackVelocity = knockbackDirection * knockbackForce;
        knockbackVelocity.y = knockbackUpwardForce;
        
        knockbackEndTime = Time.time + knockbackDuration;
    }
    
    public bool IsBlocking() => isBlocking.Value;
}