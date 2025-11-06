using UnityEngine;

public class Block : MonoBehaviour
{
    [Header("Block Settings")]
    public float blockDamageReduction = 0.8f; // blocks 80% of damage
    
    [Header("Perfect Block")]
    public bool enablePerfectBlock = true;
    public float perfectBlockWindow = 0.2f; // seconds to trigger perfect block
    
    [Header("References")]
    public Animator combatAnimator; // same animator as MeleeWeapon - has both Swing and Block triggers
    
    private bool isBlocking = false;
    private float blockStartTime;
    private bool isLocalPlayer = true;
    
    void Update()
    {
        // only process input for local player
        if (!isLocalPlayer) return;
        
        // handle block input using Input Manager - hold to block
        if (Input.GetButton("Block"))
        {
            if (!isBlocking)
            {
                StartBlock();
            }
        }
        else
        {
            if (isBlocking)
            {
                EndBlock();
            }
        }
    }
    
    void StartBlock()
    {
        isBlocking = true;
        blockStartTime = Time.time;
        
        // set blocking state in animator
        if (combatAnimator != null)
        {
            combatAnimator.SetBool("IsBlocking", true);
        }
    }
    
    void EndBlock()
    {
        isBlocking = false;
        
        // clear blocking state in animator
        if (combatAnimator != null)
        {
            combatAnimator.SetBool("IsBlocking", false);
        }
    }
    
    public void OnBlockedAttack(float incomingDamage)
    {
        if (!isBlocking) return;
        
        Debug.Log("OnBlockedAttack called. Incoming damage: " + incomingDamage);
        
        // check for perfect block
        bool isPerfectBlock = enablePerfectBlock && (Time.time - blockStartTime) <= perfectBlockWindow;
        
        if (isPerfectBlock)
        {
            // perfect block does no damage
            Debug.Log("Perfect Block!");
        }
        else
        {
            // normal block reduces damage
            float reducedDamage = incomingDamage * (1f - blockDamageReduction);
            Debug.Log("Normal block. Reduced damage: " + reducedDamage);
            
            Health health = GetComponent<Health>();
            if (health != null)
            {
                health.TakeDamage(reducedDamage);
            }
        }
    }
    
    public bool IsBlocking() => isBlocking;
    
    public void SetLocalPlayer(bool isLocal)
    {
        isLocalPlayer = isLocal;
    }
}