using FMODUnity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlayerStates
{
    Walk,
    Punch,
    Punch_Again
}

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    private Rigidbody2D rb;
    private Coroutine punchCoroutine;

    public PlayerSettings playerSettings;
    public PlayerStates playerState;

    public EventReference onPunchEvent;


    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        playerState = PlayerStates.Walk;
    }

    // Update is called once per frame
    void Update()
    {
        switch (playerState)
        {
            case PlayerStates.Walk:
                WalkState();
                break;
            case PlayerStates.Punch:
                PunchState();
                break;
        }
    }

    /********************************************************************************************/

    public void WalkState()
    {
        float moveHorizontal = Input.GetAxisRaw("Horizontal");
        float moveVertical = Input.GetAxisRaw("Vertical");

        Vector2 movement = new Vector2(moveHorizontal * playerSettings.horizontalSpeed, moveVertical * playerSettings.verticalSpeed);

        if (!Mathf.Approximately(moveHorizontal, 0f) || !Mathf.Approximately(moveVertical, 0f))
        {
            rb.velocity = movement;
        }
        else
        {
            rb.velocity = Vector2.zero;
        }

        if (Input.GetKeyDown(KeyCode.Z))
        {
            rb.velocity = Vector2.zero;
            playerState = PlayerStates.Punch;
        }
    }

    public void PunchState()
    {
        if (punchCoroutine == null)
        {
            punchCoroutine = StartCoroutine(PunchCoroutine());
        }
    }

    /********************************************************************************************/

    public IEnumerator PunchCoroutine()
    {
        RuntimeManager.PlayOneShot(onPunchEvent, transform.position);
        yield return new WaitForSeconds(playerSettings.punchCooldown);

        // TODO: Implement the attack logic here (e.g., play attack animation, detect enemies, etc.)

        playerState = PlayerStates.Walk;
        punchCoroutine = null;
    }
}
