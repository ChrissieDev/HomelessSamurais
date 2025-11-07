using UnityEngine;
using Unity.Netcode;

public class DamageDealer : NetworkBehaviour
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
        // Check layer mask
        if (((1 << target.layer) & damageableLayers) == 0) return;
        
        // Check tag if specified
        if (!string.IsNullOrEmpty(damageableTag) && !target.CompareTag(damageableTag)) return;
        
        // Try to damage
        Health health = target.GetComponent<Health>();
        if (health != null)
        {
            // Check if we need to request damage from server
            if (IsServer)
            {
                // Server can damage directly
                health.TakeDamage(damageAmount);
            }
            else if (IsClient)
            {
                // Client must request server to deal damage
                NetworkObject targetNetObj = target.GetComponent<NetworkObject>();
                if (targetNetObj != null)
                {
                    RequestDamageServerRpc(targetNetObj.NetworkObjectId, damageAmount);
                }
            }
        }
    }
    
    [ServerRpc(RequireOwnership = false)]
    void RequestDamageServerRpc(ulong targetNetworkId, float damage)
    {
        // Server validates and applies damage
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(targetNetworkId, out NetworkObject targetObject))
        {
            Health health = targetObject.GetComponent<Health>();
            if (health != null)
            {
                health.TakeDamage(damage);
            }
        }
    }
    
    // Manual call for weapons/abilities
    public void DealDamage(GameObject target)
    {
        DealDamageTo(target);
    }
}