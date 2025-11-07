using UnityEngine;
using Unity.Netcode;

public class MeleeWeapon : NetworkBehaviour
{
    [Header("Weapon Settings")]
    public float damage = 25f;
    public float attackCooldown = 1f;
    public float attackStartupDelay = 0.3f; // 300ms delay before hitbox activates
    public float attackDuration = 0.6f; // 600ms hitbox active duration
    public LayerMask hitLayers;
    
    [Header("References")]
    public Animator weaponAnimator;
    public Collider weaponCollider;
    
    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip hitSound;
    public AudioClip blockSound;
    public AudioClip swingSound;
    
    private bool isAttacking = false;
    private bool canAttack = true;
    private bool hasHitThisSwing = false;
    
    void Start()
    {
        if (weaponCollider != null)
        {
            weaponCollider.enabled = false;
        }
        
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }
    }
    
    void Update()
    {
        // Only allow local player to attack
        if (!IsOwner) return;
        
        if (Input.GetButtonDown("Attack") && canAttack && !isAttacking)
        {
            StartAttack();
        }
    }
    
    void StartAttack()
    {
        isAttacking = true;
        canAttack = false;
        hasHitThisSwing = false;
        
        // Tell server to play animation for everyone
        if (IsOwner)
        {
            StartAttackServerRpc();
        }
        
        // Enable hitbox after 300ms delay (also plays swing sound)
        Invoke(nameof(EnableHitbox), attackStartupDelay);
        
        // Disable hitbox after delay + duration (300ms + 600ms = 900ms)
        Invoke(nameof(DisableHitbox), attackStartupDelay + attackDuration);
    }
    
    void EnableHitbox()
    {
        if (weaponCollider != null)
        {
            weaponCollider.enabled = true;
        }
        
        // Play swing sound when hitbox activates (at 300ms)
        if (swingSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(swingSound);
        }
    }
    
    void DisableHitbox()
    {
        if (weaponCollider != null)
        {
            weaponCollider.enabled = false;
        }
        
        EndAttack();
    }
    
    [ServerRpc]
    void StartAttackServerRpc()
    {
        // Play animation for all clients
        StartAttackClientRpc();
    }
    
    [ClientRpc]
    void StartAttackClientRpc()
    {
        if (weaponAnimator != null)
        {
            weaponAnimator.SetTrigger("Swing");
        }
    }
    
    void EndAttack()
    {
        isAttacking = false;
        
        Invoke(nameof(ResetAttack), attackCooldown);
    }
    
    void ResetAttack()
    {
        canAttack = true;
    }
    
    public void OnWeaponHit(Collider other)
    {
        if (!isAttacking || hasHitThisSwing) return;
        
        int layerCheck = (1 << other.gameObject.layer) & hitLayers;
        if (layerCheck == 0) return;
        
        // Check if we hit their pipe (blocking)
        if (other.gameObject.name == "Pipe")
        {
            Block block = other.GetComponentInParent<Block>();
            if (block != null && block.IsBlocking())
            {
                block.OnBlockedAttack(damage, transform.position);
                hasHitThisSwing = true;
                
                if (blockSound != null && audioSource != null)
                {
                    audioSource.PlayOneShot(blockSound);
                }
                return;
            }
        }
        
        // Try to damage DummyEnemy
        DummyEnemy dummy = other.GetComponent<DummyEnemy>();
        if (dummy != null)
        {
            dummy.TakeDamage(damage);
            hasHitThisSwing = true;
            
            if (hitSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(hitSound);
            }
            return;
        }
        
        // Try to damage player - send to server
        Health health = other.GetComponentInParent<Health>();
        if (health != null)
        {
            // Request server to deal damage
            DealDamageServerRpc(health.NetworkObjectId, damage);
            hasHitThisSwing = true;
            
            if (hitSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(hitSound);
            }
        }
    }
    
    [ServerRpc]
    void DealDamageServerRpc(ulong targetNetworkId, float damageAmount)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(targetNetworkId, out NetworkObject targetObject))
        {
            Health health = targetObject.GetComponent<Health>();
            if (health != null)
            {
                health.TakeDamage(damageAmount);
            }
        }
    }
    
    public bool IsAttacking() => isAttacking;
}