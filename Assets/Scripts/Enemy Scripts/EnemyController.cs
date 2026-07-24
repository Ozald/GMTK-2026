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

    protected EnemyManager enemyManager;

    public Coroutine waitCoroutine;
    public Coroutine attackCoroutine;
    public Coroutine prepareCoroutine;
    public Rigidbody2D rb;

    private SpriteRenderer spriteRenderer;
    private Color originalColor;

    public float speed = 3.0f;
    public float stoppingDistance = 1.0f;
    public float sideOffset = 1.2f;
    public float laneTolerance = 0.25f;
    
    public float roamAmount = 0.15f;
    public float roamSpeed = 0.5f;
    public float roamMoveSpeed = 0.5f;
    private Vector3 roamOffset;

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
    }

    void Update()
    {
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
        }
    }

    #endregion

    #region States

    protected virtual void WalkState()
    {
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
        }
        else
        {
            rb.velocity = Vector2.zero;

            if (canAttack)
            {
                enemyState = EnemyStates.ReadyToAttack;
            }
            else
            {
                roamSide = side;
                roamDistance = waitingDistance;

                roamOffset = new Vector3(
                    Random.Range(-1f, 1f),
                    Random.Range(-1f, 1f),
                    0
                );

                enemyState = EnemyStates.Roaming;
            }
        }
    }

    public void AttackState()
    {
        if (enemyType == EnemyType.Melee || enemyType == EnemyType.Tank)
        {
            if (!enemyManager.CanEnemyAttack(this))
            {
                enemyState = EnemyStates.Walk;
                return;
            }

            if (enemyManager.IsOtherEnemyAttacking(this))
            {
                enemyState = EnemyStates.ReadyToAttack;
                return;
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
                enemyState = EnemyStates.PreparingAttack;
            }
        }
        else if (enemyType == EnemyType.Ranged)
        {
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
        // Keep the roaming position attached to the player
        homePosition = playerTransform.position +
                       Vector3.right * roamSide * roamDistance;

        // Small vertical movement
        float offset = Mathf.Sin(Time.time * roamSpeed + GetInstanceID()) * roamAmount;

        roamTarget = homePosition + new Vector3(0, offset, 0);

        // Get pushed away from nearby enemies
        Vector3 separation = GetSeperationForce();


        Vector3 finalTarget = roamTarget + separation;

        transform.position = Vector3.MoveTowards(
            transform.position,
            finalTarget,
            roamMoveSpeed * Time.deltaTime
        );

        // If this enemy becomes one of the attackers
        if (enemyManager.CanEnemyAttack(this))
        {
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
    #endregion

    #region Coroutines
    protected virtual IEnumerator AttackCoroutine()
    {
        //RuntimeManager.PlayOneShot(onPunchEvent, transform.position);
        yield return new WaitForSeconds(attackAnimationLength);

        // TODO: Implement the attack logic here (e.g., play attack animation, detect enemies, etc.)

        enemyState = EnemyStates.Waiting;
        attackCoroutine = null;
    }

    

    public IEnumerator WaitingCoroutine()
    {
        yield return new WaitForSeconds(attackCooldown);

        enemyState = EnemyStates.Walk;
        waitCoroutine = null;
    }

    public IEnumerator PreparingAttackCoroutine()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.yellow;
        }

        yield return new WaitForSeconds(attackWindUp);

        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
        }

        enemyState = EnemyStates.Attack;
        prepareCoroutine = null;
    }
    #endregion

    void TakeDamage(int amount)
    {
        health -= amount;
    }

    Vector3 GetSeperationForce()
    {
        Vector3 force = Vector3.zero;

        foreach(EnemyController enemy in allMeleeEnemies){
            if (enemy == this)
            {
                continue;
            }

            float distance = Vector3.Distance(transform.position, enemy.transform.position);

            if (distance < seperationDistance)
            {
                Vector3 away = transform.position - enemy.transform.position;

                if (away != Vector3.zero)
                {
                    force += away.normalized * (seperationDistance - distance);
                }
            }
        }

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
