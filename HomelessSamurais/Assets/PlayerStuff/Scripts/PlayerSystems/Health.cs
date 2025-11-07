using UnityEngine;
using UnityEngine.Events;
using Unity.Netcode;

public class Health : NetworkBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 100f;
    public bool canRegenerate = false;
    public float regenRate = 5f; // health per second
    public float regenDelay = 3f; // seconds after taking damage before regen starts
    
    [Header("Death Settings")]
    public bool destroyOnDeath = false;
    public float destroyDelay = 3f;
    
    [Header("Events")]
    public UnityEvent<float> OnHealthChanged; // passes current health
    public UnityEvent<float> OnDamageTaken; // passes damage amount
    public UnityEvent OnDeath;
    public UnityEvent OnRespawn;
    
    private NetworkVariable<float> currentHealth = new NetworkVariable<float>(
        default,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );
    
    private float timeSinceLastDamage;
    private bool isDead = false;
    
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        
        if (IsServer)
        {
            currentHealth.Value = maxHealth;
        }
        
        // Subscribe to health changes on all clients
        currentHealth.OnValueChanged += OnHealthValueChanged;
    }
    
    public override void OnNetworkDespawn()
    {
        currentHealth.OnValueChanged -= OnHealthValueChanged;
        base.OnNetworkDespawn();
    }
    
    void OnHealthValueChanged(float previousValue, float newValue)
    {
        OnHealthChanged?.Invoke(newValue);
        
        if (newValue <= 0 && !isDead)
        {
            isDead = true;
            OnDeath?.Invoke();
        }
    }
    
    void Update()
    {
        // Only server handles regeneration
        if (!IsServer) return;
        
        // handle regeneration
        if (canRegenerate && !isDead && currentHealth.Value < maxHealth)
        {
            timeSinceLastDamage += Time.deltaTime;
            
            if (timeSinceLastDamage >= regenDelay)
            {
                Heal(regenRate * Time.deltaTime);
            }
        }
    }
    
    public void TakeDamage(float damage)
    {
        // Only server can modify health
        if (!IsServer) return;
        if (isDead) return;
        
        currentHealth.Value -= damage;
        timeSinceLastDamage = 0f;
        
        // Notify clients about damage
        NotifyDamageTakenClientRpc(damage);
        
        if (currentHealth.Value <= 0)
        {
            Die();
        }
    }
    
    [ClientRpc]
    void NotifyDamageTakenClientRpc(float damage)
    {
        OnDamageTaken?.Invoke(damage);
    }
    
    public void Heal(float amount)
    {
        if (!IsServer) return;
        if (isDead) return;
        
        currentHealth.Value = Mathf.Min(currentHealth.Value + amount, maxHealth);
    }
    
    void Die()
    {
        if (!IsServer) return;
        
        isDead = true;
        currentHealth.Value = 0;
        
        OnDeath?.Invoke();
        
        if (destroyOnDeath)
        {
            Destroy(gameObject, destroyDelay);
        }
    }
    
    public void Respawn()
    {
        if (!IsServer) return;
        
        RespawnClientRpc();
    }
    
    [ClientRpc]
    void RespawnClientRpc()
    {
        isDead = false;
        timeSinceLastDamage = 0f;
        OnRespawn?.Invoke();
        
        if (IsServer)
        {
            currentHealth.Value = maxHealth;
        }
    }
    
    // getters
    public float GetCurrentHealth() => currentHealth.Value;
    public float GetMaxHealth() => maxHealth;
    public float GetHealthPercentage() => currentHealth.Value / maxHealth;
    public bool IsDead() => isDead;
}