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
    private EnemyManager enemyManager;

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
    public EnemyStates enemyState;

    private Transform playerTransform;
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
    #endregion

    #region General functions
    void OnEnable()
    {
        allEnemies.Add(this);
    }

    void OnDisable()
    {
        allEnemies.Remove(this);
    }

    void Start()
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

    public void WalkState()
    {
        Debug.Log($"{name} is in WalkState");

        bool canAttack = enemyManager.CanEnemyAttack(this);

        float side = Mathf.Sign(transform.position.x - playerTransform.position.x);

        float distanceFromPlayer;

        if (canAttack)
        {
            distanceFromPlayer = sideOffset;
        }
        else
        {
            distanceFromPlayer = waitingDistance;
        }

        Vector3 targetPosition = playerTransform.position + Vector3.right * side * distanceFromPlayer;
        targetPosition.z = playerTransform.position.z;
        float laneDifference = Mathf.Abs(transform.position.z - playerTransform.position.z);

        if (laneDifference > laneTolerance)
        {
            targetPosition = new Vector3(transform.position.x, transform.position.y, playerTransform.position.z);
        }
        else
        {
            //targetPosition = playerTransform.position + Vector3.right * side * sideOffset;
            targetPosition.z = playerTransform.position.z;
        }

        float distance = Vector3.Distance(transform.position, targetPosition);

        Debug.Log($"{name} Distance: {distance}");

        if (distance > stoppingDistance)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);
        }
        else
        {
            rb.velocity = Vector2.zero;

            Debug.Log($"{name} CanAttack = {enemyManager.CanEnemyAttack(this)}");


            if (enemyManager.CanEnemyAttack(this))
            {
                Debug.Log($"{name} -> ReadyToAttack");
                enemyState = EnemyStates.ReadyToAttack;
            }
            else
            {
                Debug.Log($"{name} -> Roaming");
                homePosition = targetPosition;

                roamSide = Mathf.Sign(transform.position.x - playerTransform.position.x);
                roamDistance = waitingDistance;

                enemyState = EnemyStates.Roaming;
            }
        }
    }

    public void AttackState()
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

        if (attackCoroutine == null)
        {
            attackCoroutine = StartCoroutine(AttackCoroutine());
        }
    }

    public void ReadyToAttackState()
    {
        Debug.Log("I am Ready To Attack");
        if (!enemyManager.IsOtherEnemyAttacking(this))
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

        transform.position = Vector3.MoveTowards(
            transform.position,
            roamTarget,
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
    public IEnumerator AttackCoroutine()
    {
        //RuntimeManager.PlayOneShot(onPunchEvent, transform.position);
        yield return new WaitForSeconds(0.75f);

        // TODO: Implement the attack logic here (e.g., play attack animation, detect enemies, etc.)

        enemyState = EnemyStates.Waiting;
        attackCoroutine = null;
    }

    

    public IEnumerator WaitingCoroutine()
    {
        yield return new WaitForSeconds(0.5f);

        enemyState = EnemyStates.Walk;
        waitCoroutine = null;
    }

    public IEnumerator PreparingAttackCoroutine()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.yellow;
        }

        yield return new WaitForSeconds(0.3f);

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
}
