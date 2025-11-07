using UnityEngine;

public class DummyEnemy : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 100f;
    private float currentHealth;
    
    void Start()
    {
        currentHealth = maxHealth;
        Debug.Log($"Dummy spawned with {maxHealth} HP");
    }
    
    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        Debug.Log($"Dummy took {damage} damage. Current HP: {currentHealth}/{maxHealth}");
        
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    void Die()
    {
        Debug.Log("Dummy destroyed!");
        Destroy(gameObject);
    }
}