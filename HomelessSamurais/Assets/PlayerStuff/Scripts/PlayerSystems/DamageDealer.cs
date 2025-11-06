using UnityEngine;

public class DamageDealer : MonoBehaviour
{
    [Header("Damage Settings")]
    public float damageAmount = 10f;
    public bool damageOnTrigger = true;
    public bool damageOnCollision = false;
    public bool continuousDamage = false; // damages over time while in contact
    public float damageInterval = 0.5f; // time between continuous damage ticks
    
    [Header("Filters")]
    public LayerMask damageableLayers;
    public string damageableTag = ""; // optional tag filter
    
    private float lastDamageTime;
    
    void OnTriggerEnter(Collider other)
    {
        if (!damageOnTrigger) return;
        DealDamageTo(other.gameObject);
    }
    
    void OnTriggerStay(Collider other)
    {
        if (!damageOnTrigger || !continuousDamage) return;
        
        if (Time.time >= lastDamageTime + damageInterval)
        {
            DealDamageTo(other.gameObject);
            lastDamageTime = Time.time;
        }
    }
    
    void OnCollisionEnter(Collision collision)
    {
        if (!damageOnCollision) return;
        DealDamageTo(collision.gameObject);
    }
    
    void OnCollisionStay(Collision collision)
    {
        if (!damageOnCollision || !continuousDamage) return;
        
        if (Time.time >= lastDamageTime + damageInterval)
        {
            DealDamageTo(collision.gameObject);
            lastDamageTime = Time.time;
        }
    }
    
    void DealDamageTo(GameObject target)
    {
        // check layer mask
        if (((1 << target.layer) & damageableLayers) == 0) return;
        
        // check tag if specified
        if (!string.IsNullOrEmpty(damageableTag) && !target.CompareTag(damageableTag)) return;
        
        // try to damage
        Health health = target.GetComponent<Health>();
        if (health != null)
        {
            health.TakeDamage(damageAmount);
        }
    }
    
    // manual call for damage
    public void DealDamage(GameObject target)
    {
        DealDamageTo(target);
    }
}