using UnityEngine;
using UnityEngine.Events;

public class Health : MonoBehaviour
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
    
    private float currentHealth;
    private float timeSinceLastDamage;
    private bool isDead = false;
    
    void Start()
    {
        currentHealth = maxHealth;
    }
    
    void Update()
    {
        // handle regeneration
        if (canRegenerate && !isDead && currentHealth < maxHealth)
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
        if (isDead) return;
        
        currentHealth -= damage;
        timeSinceLastDamage = 0f; // reset regen timer
        
        OnDamageTaken?.Invoke(damage);
        OnHealthChanged?.Invoke(currentHealth);
        
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    public void Heal(float amount)
    {
        if (isDead) return;
        
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        OnHealthChanged?.Invoke(currentHealth);
    }
    
    void Die()
    {
        isDead = true;
        currentHealth = 0;
        
        OnDeath?.Invoke();
        
        if (destroyOnDeath)
        {
            Destroy(gameObject, destroyDelay);
        }
    }
    
    public void Respawn()
    {
        isDead = false;
        currentHealth = maxHealth;
        timeSinceLastDamage = 0f;
        
        OnRespawn?.Invoke();
        OnHealthChanged?.Invoke(currentHealth);
    }
    
    // getters
    public float GetCurrentHealth() => currentHealth;
    public float GetMaxHealth() => maxHealth;
    public float GetHealthPercentage() => currentHealth / maxHealth;
    public bool IsDead() => isDead;
}