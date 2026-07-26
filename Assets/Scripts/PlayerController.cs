using FMODUnity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlayerStates
{
    Walk,
    Attack,
    Dash,
    Parry,
    Hit
}

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [HideInInspector] public Rigidbody2D rb;
    private Coroutine attackCoroutine;
    private float stateTimer;
    private AttackData currentAttack;
    private bool attackQueued;
    private bool IsWalking;
    private Animator animator;
    private AnimatorOverrideController overrideController;

    [Header("Movement Bounds")]
    [SerializeField] private float minX = -8f;
    [SerializeField] private float maxX = 8f;

    public PlayerSettings playerSettings;
    public PlayerStates playerState;

    // Start is called before the first frame update
    void Start()
    {
        ResetState();
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        overrideController = new AnimatorOverrideController(animator.runtimeAnimatorController);
        animator.runtimeAnimatorController = overrideController;

        playerState = PlayerStates.Walk;
    }

    // Update is called once per frame
    void Update()
    {
        stateTimer += Time.deltaTime;

        if (ComboManager.Instance.currentCombo > 0)
        {
            switch (playerState)
            {
                case PlayerStates.Walk:
                    WalkState();
                    break;
                case PlayerStates.Attack:
                    AttackState();
                    break;
                case PlayerStates.Dash:
                    DashState();
                    break;
                case PlayerStates.Parry:
                    ParryState();
                    break;
                case PlayerStates.Hit:
                    HitState();
                    break;
            }
        }
        else
        {
            animator.Play("PlayerDeath", -1, 0f);
        }
        ClampPosition();
    }

    /********************************************************************************************/

    public void WalkState()
    {
        float moveHorizontal = Input.GetAxisRaw("Horizontal");
        float moveVertical = Input.GetAxisRaw("Vertical");

        Vector2 movement = new Vector2(moveHorizontal * playerSettings.horizontalSpeed, moveVertical * playerSettings.verticalSpeed);


        bool isMoving = movement.sqrMagnitude > 0.01f;

        animator.SetBool("IsWalking", isMoving);

        if (isMoving)
        {
            rb.velocity = movement;
            
            transform.localScale = new Vector3(movement.x > 0 ? 1 : -1, 1, 1);
        }
        else
        {
            rb.velocity = Vector2.zero;
        }

        if (Input.GetKeyDown(KeyCode.Z))
        {
            ResetState();

            rb.velocity = Vector2.zero;
            currentAttack = playerSettings.punchAttack;
            playerState = PlayerStates.Attack;

            return;
        }

        if (Input.GetKeyDown(KeyCode.X))
        {
            ResetState();
            rb.velocity = Vector2.zero;
            animator.SetTrigger("ParryStart");
            playerState = PlayerStates.Parry;
            return; 
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            ResetState();

            rb.velocity = Vector2.zero;
            rb.AddForce(movement.normalized * playerSettings.dashForce, ForceMode2D.Impulse);

            animator.Play("Dash", -1, 0f);
            playerState = PlayerStates.Dash;

            AudioManager.PlayOneShot(playerSettings.dashSound);

            return;
        }
    }

    public void AttackState()
    {
        animator.SetBool("IsWalking", false);

        if (attackCoroutine == null)
        {
            StopCoroutine(AttackCoroutine());
            attackCoroutine = StartCoroutine(AttackCoroutine());
        }

        if (Input.GetKeyDown(KeyCode.Z) && currentAttack.IsInComboWindow(stateTimer))
        {
            attackQueued = true;
        }

        if (Input.GetKeyDown(KeyCode.Space) && attackCoroutine != null)
        {
            ResetState();

            float moveHorizontal = Input.GetAxisRaw("Horizontal");
            float moveVertical = Input.GetAxisRaw("Vertical");

            Vector2 movement = new Vector2(moveHorizontal * playerSettings.horizontalSpeed, moveVertical * playerSettings.verticalSpeed);

            transform.localScale = new Vector3(movement.x > 0 ? 1 : -1, 1, 1);

            rb.velocity = Vector2.zero;
            rb.AddForce(movement.normalized * playerSettings.dashForce, ForceMode2D.Impulse);

            animator.Play("Dash", -1, 0f);
            playerState = PlayerStates.Dash;

            AudioManager.PlayOneShot(playerSettings.dashSound);

            if (attackCoroutine != null)
                StopCoroutine(attackCoroutine);

            return;
        }
    }

    public void DashState()
    {
        animator.SetBool("IsWalking", false);

        if (stateTimer >= playerSettings.dashDuration)
        {
            ResetState();
            animator.Play("PlayerWalk", -1, 0f);
            playerState = PlayerStates.Walk;
        }

        if (Input.GetKeyDown(KeyCode.X))
        {
            ResetState();
            animator.SetTrigger("ParryStart");
            playerState = PlayerStates.Parry;
        }
    }

    public void ParryState()
    {
        animator.SetBool("IsWalking", false);

        if (stateTimer >= playerSettings.parryDuration)
        {
            ResetState();
            animator.SetBool("IsWalking", true);
            playerState = PlayerStates.Walk;
        }
    }

    public void HitState()
    {
        animator.SetBool("IsWalking", false);
        if (stateTimer >= playerSettings.stunDuration)
        {
            ResetState();
            animator.SetBool("IsWalking", true);
            playerState = PlayerStates.Walk;
        }
    }

    /********************************************************************************************/

    public IEnumerator AttackCoroutine()
    {
        AudioManager.PlayOneShot(currentAttack.attackSound);

        OverrideClip("DummyAttack", currentAttack.animation);
        animator.Play("AttackState", -1, 0f);

        currentAttack.Attack(this);
        
        yield return new WaitForSeconds(currentAttack.comboWindowEnd);


        stateTimer = 0f;
        if (attackQueued && currentAttack.nextAttack != null)
        {
            AttackData nextAttack = currentAttack.nextAttack;

            attackQueued = false;
            currentAttack = nextAttack;
            playerState = PlayerStates.Attack;

            attackCoroutine = null;

            yield break;
        }

        ResetState();
        currentAttack = null;
        playerState = PlayerStates.Walk;
        
    }

    private void ResetState()
    {
        stateTimer = 0f;
        attackQueued = false;
        attackCoroutine = null;
    }

    private void OverrideClip(string originalClipName, AnimationClip newClip)
    {
        var overrides = new List<KeyValuePair<AnimationClip, AnimationClip>>(overrideController.overridesCount);
        overrideController.GetOverrides(overrides);

        for (int i = 0; i < overrides.Count; i++)
        {
            if (overrides[i].Key != null && overrides[i].Key.name == originalClipName)
            {
                overrides[i] = new KeyValuePair<AnimationClip, AnimationClip>(overrides[i].Key, newClip);
                break;
            }
        }

        overrideController.ApplyOverrides(overrides);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if ((playerSettings.enemyAttackLayer.value & (1 << collision.gameObject.layer)) != 0)
        {
            if (transform.position.y > collision.transform.position.y + 0.5f || transform.position.y < collision.transform.position.y - 0.5f)
                return;

            Debug.Log("Player hit by enemy attack!");

            if (playerState == PlayerStates.Parry && stateTimer <= playerSettings.parryWindow)
            {
                Debug.Log("Parried!");

                EnemyController enemy = collision.GetComponentInParent<EnemyController>();

                if (enemy != null)
                {
                    ResetState();

                    StartCoroutine(ParrySuccess());

                    AudioManager.PlayOneShot(playerSettings.parrySound);
                    enemy.TakeDamage(1, playerSettings.parryKnockback);
                    ComboManager.ComboAdd(1000);

                    animator.SetBool("IsWalking", false);
                    playerState = PlayerStates.Walk;
                }

                return;
            }

            if (playerState == PlayerStates.Hit)
                return;


            ResetState();
            rb.velocity = Vector2.zero;
            animator.SetTrigger("Hit");
            playerState = PlayerStates.Hit;

            int finalDamage = Mathf.RoundToInt(200 * DifficultyManager.Instance.enemyDamageMultiplier);

            ComboManager.TakeDamage(finalDamage);
        }
    }

    private IEnumerator ParrySuccess()
    {
        animator.SetTrigger("ParrySuccess");
        yield return 0;

        Time.timeScale = 0f;
        yield return new WaitForSecondsRealtime(0.5f);
        Time.timeScale = 1f;
    }

    private void ClampPosition()
    {
        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, minX, maxX);
        transform.position = pos;
    }
}
