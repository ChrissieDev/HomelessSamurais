using UnityEngine;

public class WeaponCollider : MonoBehaviour
{
    private MeleeWeapon meleeWeapon;
    
    void Start()
    {
        Debug.Log($"WeaponCollider Start on: {gameObject.name}");
        
        // find the MeleeWeapon script on parent
        meleeWeapon = GetComponentInParent<MeleeWeapon>();
        if (meleeWeapon == null)
        {
            Debug.LogError("WeaponCollider couldn't find MeleeWeapon in parent!");
        }
        else
        {
            Debug.Log($"WeaponCollider found MeleeWeapon on: {meleeWeapon.gameObject.name}");
        }
        
        // Check collider setup
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            Debug.Log($"Collider found: Type={col.GetType().Name}, IsTrigger={col.isTrigger}, Enabled={col.enabled}");
        }
        else
        {
            Debug.LogError("NO COLLIDER on this GameObject!");
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        Debug.Log($"WeaponCollider.OnTriggerEnter: Hit {other.gameObject.name}");
        
        if (meleeWeapon != null)
        {
            meleeWeapon.OnWeaponHit(other);
        }
        else
        {
            Debug.LogError("MeleeWeapon is null in OnTriggerEnter!");
        }
    }
}