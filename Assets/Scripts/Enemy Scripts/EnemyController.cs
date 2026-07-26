using FMOD.Studio;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Playables;

public class EnemyController : MonoBehaviour
{
    #region variables

    public static List<EnemyController> allEnemies = new List<EnemyController>();
    public static List<EnemyController> allMeleeEnemies = new List<EnemyController>();

    [Header("Movement Bounds")]
    [SerializeField] private float minY = -5;
    [SerializeField] private float maxY = 0.5f;
    [SerializeField] private float borderBuffer = 0.5f;

    [Header("Combo Rewards")]
    public int meleeComboReward = 50;
    public int rangedComboReward = 75;
    public int tankComboReward = 100;

    protected EnemyManager enemyManager;

    public Coroutine waitCoroutine;
    public Coroutine attackCoroutine;
    public Coroutine prepareCoroutine;
    private Coroutine hitFlashCoroutine;
    private Coroutine interruptFlashCoroutine;
    public Rigidbody2D rb;

    private bool isKnockedBack = false;

    private SpriteRenderer spriteRenderer;
    private Color originalColor;

    public float hitStunDuration = 0.2f;
    public float interruptKnockbackForce = 1.5f;

    private Coroutine hitCoroutine;
    private EnemyStates previousState;

    public float speed = 3.0f;
    public float stoppingDistance = 1.0f;
    public float sideOffset = 1.2f;
    public float laneTolerance = 0.25f;


    public float maxRoamDistance = 2f;
    public float roamAmount = 0.15f;
    public float roamSpeed = 0.5f;
    public float roamMoveSpeed = 0.5f;
    private Vector3 roamOffset;
    private Vector3 roamStartPlayerPosition;

    public int maxHitsBeforeAttack = 3;
    private int hitCount = 0;
    private bool canBeInterrupted = true;

    float roamSide;
    float roamDistance;


    private Vector3 roamTarget;
    private Vector3 homePosition;

    public float seperationDistance = 1.0f;
    public float seperationStrength = 2.0f;

    public float waitingDistance = 4f;
    public float attackDistance = 1.2f;

    public float health = 5;
    public float damage = 1;

    public float attackAnimationLength = 0.3f;
    public float attackCooldown = 0.5f;
    public float attackWindUp = 0.3f;
    public EnemyStates enemyState;

    protected Transform playerTransform;

    private Animator animator;
    private AnimatorOverrideController overrideController;

    public Material flashMaterial;

    public enum EnemyStates
    {
        Walk,
        ReadyToAttack,
        PreparingAttack,
        Attack,
        Waiting,
        Roaming,
        Hit,
        Dead
    }

    public enum EnemyType
    {
        Melee,
        Ranged,
        Tank
    }

    public EnemyType enemyType;
    #endregion

    #region General functions
    void OnEnable()
    {
        allEnemies.Add(this);
        if (enemyType == EnemyType.Melee || enemyType == EnemyType.Tank)
        {
            allMeleeEnemies.Add(this);
        }
    }
    void OnDisable()
    {
        allEnemies.Remove(this);

        if (enemyType == EnemyType.Melee || enemyType == EnemyType.Tank)
        {
            allMeleeEnemies.Remove(this);
        }
    }

    protected virtual void Start()
    {

        rb = GetComponent<Rigidbody2D>();

        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }

        GameObject player = GameObject.FindWithTag("Player");
        if(player != null)
        {
            playerTransform = player.transform;
        }
        enemyManager = FindObjectOfType<EnemyManager>();
        enemyState = EnemyStates.Walk;

        animator = GetComponent<Animator>();

        overrideController = new AnimatorOverrideController(animator.runtimeAnimatorController);
        animator.runtimeAnimatorController = overrideController;
    }

    void Update()
    {
        ClampYPosition();
        FacePlayer();

        switch (enemyState)
        {
            case EnemyStates.Walk:
                WalkState();
                break;
            case EnemyStates.Waiting:
                WaitState();
                break;
            case EnemyStates.Roaming:
                RoamState();
                break;
            case EnemyStates.ReadyToAttack:
                ReadyToAttackState();
                break;
            case EnemyStates.PreparingAttack:
                PreparingAttackState();
                break;
            case EnemyStates.Attack:
                AttackState();
                break;
            case EnemyStates.Hit:
                HitState();
                break;
        }


    }

    private void FixedUpdate()
    {
        ClampYPosition();
    }

    private void ClampYPosition()
    {
        Vector3 pos = transform.position;

        pos.y = Mathf.Clamp(pos.y, minY, maxY);

        transform.position = pos;
    }

    //private void OnTriggerEnter2D(Collider2D collision)
    //{
    //    if (collision.CompareTag("PlayerHitbox"))
    //    {
    //        AttackHitbox hitbox = collision.GetComponent<AttackHitbox>();

    //        if (hitbox == null)
    //            return;

    //        if (TakeDamage(hitbox.damage))
    //        {

    //            ComboManager.ComboAdd(hitbox.comboGain);
    //        }
    //    }
    //}

    #endregion

    #region States

    protected virtual void WalkState()
    {
        if (isKnockedBack)
            return;

        bool canAttack = enemyManager.CanEnemyAttack(this);

        float side = Mathf.Sign(transform.position.x - playerTransform.position.x);

        float distanceFromPlayer = canAttack ? sideOffset : waitingDistance;

        Vector3 targetPosition;

        if (canAttack)
        {
            // Attackers care about lane
            targetPosition = playerTransform.position +
                             Vector3.right * side * distanceFromPlayer;

            targetPosition.y = playerTransform.position.y;
        }
        else
        {
            // Waiting enemies only care about staying near the player
            targetPosition = playerTransform.position +
                             Vector3.right * side * distanceFromPlayer;

            // Keep their current lane
            targetPosition.y = transform.position.y;
        }


        float distance = Vector3.Distance(
            transform.position,
            targetPosition
        );


        if (distance > stoppingDistance)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                targetPosition,
                speed * Time.deltaTime
            );

            Vector3 pos = transform.position;
            pos.y = Mathf.Clamp(pos.y, minY, maxY);
            transform.position = pos;
        }
        else
        {
            rb.velocity = Vector2.zero;

            if (canAttack)
            {
                animator.SetBool("IsWalking", false);
                enemyState = EnemyStates.ReadyToAttack;
            }
            else
            {
                roamSide = side;
                roamDistance = waitingDistance;
                roamStartPlayerPosition = playerTransform.position;

                roamOffset = new Vector3(
                    Random.Range(-1f, 1f),
                    Random.Range(-1f, 1f),
                    0
                );

                animator.SetBool("IsWalking", true);
                enemyState = EnemyStates.Roaming;
            }
        }
    }

    public void AttackState()
    {
        // Ignore enemy manager restrictions when enraged after max hits
        if (canBeInterrupted)
        {
            if (enemyType == EnemyType.Melee || enemyType == EnemyType.Tank)
            {
                if (!enemyManager.CanEnemyAttack(this))
                {
                    animator.SetBool("IsWalking", true);
                    enemyState = EnemyStates.Walk;
                    return;
                }

                if (enemyManager.IsOtherEnemyAttacking(this))
                {
                    animator.SetBool("IsWalking", false);
                    enemyState = EnemyStates.ReadyToAttack;
                    return;
                }
            }
        }

        if (attackCoroutine == null)
        {
            attackCoroutine = StartCoroutine(AttackCoroutine());
        }
    }

    public void ReadyToAttackState()
    {
        if (enemyType == EnemyType.Melee || enemyType == EnemyType.Tank)
        {
            if (!enemyManager.IsOtherEnemyAttacking(this))
            {
                animator.SetBool("IsWalking", false);
                enemyState = EnemyStates.PreparingAttack;
            }
        }
        else if (enemyType == EnemyType.Ranged)
        {
            animator.SetBool("IsWalking", false);
            enemyState = EnemyStates.PreparingAttack;
        }
    }

    public void PreparingAttackState()
    {
        if (prepareCoroutine == null)
        {
            prepareCoroutine = StartCoroutine(PreparingAttackCoroutine());
        }
    }

    void RoamState()
    {

        if (isKnockedBack)
            return;

        // If the player has moved too far away, chase them again.
        float distanceToHome = Vector3.Distance(transform.position, homePosition);

        if (Vector3.Distance(playerTransform.position, roamStartPlayerPosition) > maxRoamDistance)
        {
            animator.SetBool("IsWalking", true);
            enemyState = EnemyStates.Walk;
            return;
        }

        // Keep the roaming position attached to the player
        homePosition = playerTransform.position +
                       Vector3.right * roamSide * roamDistance;

        float offset = Mathf.Sin(Time.time * roamSpeed + GetInstanceID()) * roamAmount;
        roamTarget = homePosition + new Vector3(0, offset, 0);

        Vector3 separation = GetSeperationForce();

        float topLimit = maxY - borderBuffer;
        float bottomLimit = minY + borderBuffer;

        // Stop vertical separation when near the top/bottom
        if (transform.position.y >= topLimit && separation.y > 0)
        {
            separation.y = 0;
        }

        if (transform.position.y <= bottomLimit && separation.y < 0)
        {
            separation.y = 0;
        }

        Vector3 finalTarget = roamTarget + separation;

        finalTarget.y = Mathf.Clamp(finalTarget.y, minY, maxY);

        transform.position = Vector3.MoveTowards(
            transform.position,
            finalTarget,
            roamMoveSpeed * Time.deltaTime
        );

        Vector3 pos = transform.position;
        pos.y = Mathf.Clamp(pos.y, minY, maxY);
        transform.position = pos;

        if (enemyManager.CanEnemyAttack(this))
        {
            animator.SetBool("IsWalking", true);
            enemyState = EnemyStates.Walk;
        }
    }

    public void WaitState()
    {
        if (waitCoroutine == null)
        {
            waitCoroutine = StartCoroutine(WaitingCoroutine());
        }
    }

    private void HitState()
    {
        if (hitCoroutine == null)
        {
            hitCoroutine = StartCoroutine(HitCoroutine());
        }
    }
    #endregion

    #region Coroutines
    protected virtual IEnumerator AttackCoroutine()
    {
        animator.Play("AttackState", -1, 0f);

        yield return new WaitForSeconds(attackAnimationLength);

        hitCount = 0;
        canBeInterrupted = true;

        animator.SetBool("IsWalking", false);
        enemyState = EnemyStates.Waiting;
        attackCoroutine = null;
    }



    public IEnumerator WaitingCoroutine()
    {
        yield return new WaitForSeconds(attackCooldown);

        animator.SetBool("IsWalking", true);
        enemyState = EnemyStates.Walk;
        waitCoroutine = null;
    }

    public IEnumerator PreparingAttackCoroutine()
    {
        yield return new WaitForSeconds(attackWindUp);

        animator.SetBool("IsWalking", false);
        enemyState = EnemyStates.Attack;
        prepareCoroutine = null;
    }

    private IEnumerator HitCoroutine()
    {
        // Stop attack
        if (attackCoroutine != null)
        {
            StopCoroutine(attackCoroutine);
            attackCoroutine = null;
        }

        if (prepareCoroutine != null)
        {
            StopCoroutine(prepareCoroutine);
            prepareCoroutine = null;
        }


        isKnockedBack = true;

        yield return new WaitForSeconds(hitStunDuration);


        if (rb != null)
        {
            rb.velocity = Vector2.zero;
        }


        isKnockedBack = false;


        if (enemyState != EnemyStates.Dead)
        {
            enemyState = previousState;
        }


        hitCoroutine = null;
    }
    #endregion

    public bool TakeDamage(int amount, float knockback)
    {
        if (enemyState == EnemyStates.Dead)
            return false;


        health -= amount;

        hitCount++;

        bool rageHit = hitCount > maxHitsBeforeAttack;


        // Only do normal hit flash if it is NOT the rage hit
        if (!rageHit)
        {
            if (hitFlashCoroutine != null)
                StopCoroutine(hitFlashCoroutine);

            hitFlashCoroutine = StartCoroutine(HitFlash());
        }


        // Stagger system
        if (!rageHit && canBeInterrupted)
        {
            previousState = enemyState;
            animator.SetBool("IsWalking", false);

            if (rb != null && playerTransform != null)
            {
                float direction = Mathf.Sign(transform.position.x - playerTransform.position.x);

                // Pure horizontal knockback
                Vector2 knockbackDirection = new Vector2(direction, 0);

                rb.AddForce(knockbackDirection * knockback, ForceMode2D.Impulse);
            }

            enemyState = EnemyStates.Hit;
        }
        else if (rageHit)
        {
            canBeInterrupted = false;

            // Start attack preparation instead of hit state
            animator.SetBool("IsWalking", false);
            enemyState = EnemyStates.PreparingAttack;
        }


        if (health <= 0)
            Die();

        return true;
    }


    private IEnumerator HitFlash()
    {
        if (spriteRenderer == null || flashMaterial == null)
            yield break;

        Material originalMaterial = spriteRenderer.material;
        Color originalColor = spriteRenderer.color;

        spriteRenderer.material = flashMaterial;
        spriteRenderer.color = Color.white;

        yield return new WaitForSeconds(0.1f);

        spriteRenderer.material = originalMaterial;
        spriteRenderer.color = originalColor;

        hitFlashCoroutine = null;
    }

    private IEnumerator InterruptFlash()
    {
        if (spriteRenderer == null)
            yield break;

        spriteRenderer.color = Color.red;

        yield return new WaitForSeconds(0.2f);

        if (enemyState != EnemyStates.Dead)
            spriteRenderer.color = originalColor;

        interruptFlashCoroutine = null;
    }

    private void Die()
    {
        enemyState = EnemyStates.Dead;

        KillTracker.Instance.EnemyKilled();

        StopAllCoroutines();

        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.simulated = false;
        }

        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
            col.enabled = false;

        // TODO:
        // Play death animation
        // Spawn particles
        // Play sound
        // Drop loot

        hitCount = 0;
        canBeInterrupted = false;

        int comboReward = 0;

        switch (enemyType)
        {
            case EnemyType.Melee:
                comboReward = meleeComboReward;
                break;

            case EnemyType.Ranged:
                comboReward = rangedComboReward;
                break;

            case EnemyType.Tank:
                comboReward = tankComboReward;
                break;
        }
        
        float killMultiplier = KillTracker.Instance.GetKillMultiplier();

        ComboManager.ComboAdd(Mathf.RoundToInt(comboReward * killMultiplier));

        string enemyName = enemyType switch
        {
            EnemyType.Melee => "Grunt",
            EnemyType.Ranged => "Gunner",
            EnemyType.Tank => "Tank",
            _ => "Enemy"
        };

        CombatFeed.Instance.Add($"{enemyName} Defeated!");

        Destroy(gameObject);
    }

    Vector3 GetSeperationForce()
    {
        Vector3 force = Vector3.zero;

        foreach (EnemyController enemy in allMeleeEnemies)
        {
            if (enemy == this)
                continue;

            float distance = Vector3.Distance(transform.position, enemy.transform.position);

            if (distance < seperationDistance)
            {
                Vector3 away = transform.position - enemy.transform.position;

                if (away.sqrMagnitude > 0.0001f)
                {
                    away.Normalize();

                    // Reduce vertical pushing but keep some
                    away.y *= 0.25f;

                    force += away * (seperationDistance - distance);
                }
            }
        }

        // Prevent separation from being too extreme
        force.y = Mathf.Clamp(force.y, -0.5f, 0.5f);

        return force * seperationStrength;
    }
    protected void FacePlayer()
    {
        if (playerTransform == null)
            return;

        float direction = playerTransform.position.x - transform.position.x;

        if (direction > 0)
        {
            // Player is to the right
            transform.rotation = Quaternion.Euler(0, 0, 0);
        }
        else if (direction < 0)
        {
            // Player is to the left
            transform.rotation = Quaternion.Euler(0, 180, 0);
        }
    }
}
