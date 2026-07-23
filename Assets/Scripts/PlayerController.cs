using FMODUnity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlayerStates
{
    Walk,
    Attack,
    Dash
}

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    private Rigidbody2D rb;
    private Coroutine attackCoroutine;
    private float stateTimer;
    private AttackData currentAttack;
    private bool attackQueued;

    public PlayerSettings playerSettings;
    public PlayerStates playerState;

    // Start is called before the first frame update
    void Start()
    {
        ResetState();
        rb = GetComponent<Rigidbody2D>();
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
        }
    }

    /********************************************************************************************/

    public void WalkState()
    {
        float moveHorizontal = Input.GetAxisRaw("Horizontal");
        float moveVertical = Input.GetAxisRaw("Vertical");

        Vector2 movement = new Vector2(moveHorizontal * playerSettings.horizontalSpeed, moveVertical * playerSettings.verticalSpeed);

        if (movement.sqrMagnitude > 0.01)
        {
            rb.velocity = movement;
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
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            ResetState();
            rb.velocity = Vector2.zero;

            rb.AddForce(movement.normalized * playerSettings.dashForce, ForceMode2D.Impulse);
            playerState = PlayerStates.Dash;
        }
    }

    public void AttackState()
    {
        if (attackCoroutine == null)
        {
            attackCoroutine = StartCoroutine(AttackCoroutine());
        }

        if (Input.GetKeyDown(KeyCode.Z) && currentAttack.IsInComboWindow(stateTimer))
        {
            attackQueued = true;
        }
    }

    public void DashState()
    {
        if (stateTimer >= playerSettings.dashDuration)
        {
            ResetState();
            playerState = PlayerStates.Walk;
        }
    }

    /********************************************************************************************/

    public IEnumerator AttackCoroutine()
    {
        AudioManager.PlayOneShot(currentAttack.attackSound);

        Debug.Log($"Attack: {currentAttack.animationTriggerName}");

        // TODO: Implement the attack logic here (e.g., play attack animation, detect enemies, etc.)
        // THE FOLLOW LINE IS PLACEHOLDER BEHAVIOR ONLY
        ComboManager.ComboAdd(1);

        yield return new WaitForSeconds(currentAttack.comboWindowEnd);

        
        stateTimer = 0f;
        if (attackQueued && currentAttack.nextAttack != null)
        {
            AttackData nextAttack = currentAttack.nextAttack;

            ResetState();

            currentAttack = nextAttack;
            playerState = PlayerStates.Attack;

            yield break;
        }

        ResetState();
        playerState = PlayerStates.Walk;
        
    }

    private void ResetState()
    {
        stateTimer = 0f;
        attackQueued = false;
        currentAttack = null;
        attackCoroutine = null;
    }
}
