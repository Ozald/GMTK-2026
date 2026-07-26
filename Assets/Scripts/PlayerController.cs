using FMODUnity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlayerStates
{
    Walk,
    Attack,
    Dash,
    Parry
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
        }
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
            playerState = PlayerStates.Parry;
            return; 
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            ResetState();

            rb.velocity = Vector2.zero;
            rb.AddForce(movement.normalized * playerSettings.dashForce, ForceMode2D.Impulse);
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

            rb.velocity = Vector2.zero;
            rb.AddForce(movement.normalized * playerSettings.dashForce, ForceMode2D.Impulse);
            playerState = PlayerStates.Dash;

            AudioManager.PlayOneShot(playerSettings.dashSound);
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
            playerState = PlayerStates.Walk;
        }

        if (Input.GetKeyDown(KeyCode.X))
        {
            ResetState();
            playerState = PlayerStates.Parry;
        }
    }

    public void ParryState()
    {
        animator.SetBool("IsWalking", false);

        if (stateTimer >= playerSettings.parryDuration)
        {
            ResetState();
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

                    enemy.TakeDamage(0, playerSettings.parryKnockback);
                    ComboManager.ComboAdd(500);

                    playerState = PlayerStates.Walk;
                }

                return;
            }

            ComboManager.TakeDamage(200);
        }
        
    }
}
