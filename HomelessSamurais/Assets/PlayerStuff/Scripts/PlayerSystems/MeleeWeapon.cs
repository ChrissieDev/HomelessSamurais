using UnityEngine;

public class MeleeWeapon : MonoBehaviour
{
    [Header("Weapon Settings")]
    public float damage = 25f;
    public float attackCooldown = 1f;
    public float attackDuration = 0.3f; // should match your animation length
    public LayerMask hitLayers;
    
    [Header("References")]
    public Animator weaponAnimator; // animator on the pipe
    public Collider weaponCollider; // trigger collider on the pipe
    
    private bool isAttacking = false;
    private bool canAttack = true;
    private bool hasHitThisSwing = false;
    
    void Start()
    {
        // disable collider when not attacking
        if (weaponCollider != null)
        {
            weaponCollider.enabled = false;
        }
    }
    
    void Update()
    {
        // handle attack input - use Input Manager "Attack" axis
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
        
        // trigger animation
        if (weaponAnimator != null)
        {
            weaponAnimator.SetTrigger("Swing");
            Debug.Log("Swing animation triggered");
        }
        
        // enable weapon collider
        if (weaponCollider != null)
        {
            weaponCollider.enabled = true;
        }
        
        // end attack after duration
        Invoke(nameof(EndAttack), attackDuration);
    }
    
    void EndAttack()
    {
        isAttacking = false;
        
        // disable weapon collider
        if (weaponCollider != null)
        {
            weaponCollider.enabled = false;
        }
        
        // start cooldown
        Invoke(nameof(ResetAttack), attackCooldown);
    }
    
    void ResetAttack()
    {
        canAttack = true;
    }
    
    void OnTriggerEnter(Collider other)
    {
        // only hit once per swing
        if (!isAttacking || hasHitThisSwing) return;
        
        // check layer
        if (((1 << other.gameObject.layer) & hitLayers) == 0) return;
        
        // check if we hit their pipe (blocking)
        if (other.gameObject.name == "Pipe")
        {
            Block block = other.GetComponentInParent<Block>();
            if (block != null && block.IsBlocking())
            {
                block.OnBlockedAttack(damage);
                hasHitThisSwing = true;
                Debug.Log("Hit blocked by pipe!");
                return;
            }
        }
        
        // deal damage to player body
        Health health = other.GetComponentInParent<Health>();
        if (health != null)
        {
            health.TakeDamage(damage);
            hasHitThisSwing = true;
            Debug.Log("Hit player! Damage: " + damage);
        }
    }
    
    public bool IsAttacking() => isAttacking;
}