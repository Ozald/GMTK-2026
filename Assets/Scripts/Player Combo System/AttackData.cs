using FMODUnity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

[CreateAssetMenu(fileName = "AttackData", menuName = "ScriptableObjects/Attack Data", order = 1)]
public class AttackData : ScriptableObject
{
    [Header("Attack FX")]
    public AnimationClip animation;
    public EventReference attackSound;

    [Header("Attack Properties")]
    public float damage;
    public float comboWindowStart;
    public float comboWindowEnd;

    [Header("Combo Properties")]
    public AttackData nextAttack;

    public bool IsInComboWindow(float stateTimer)
    {
        if (stateTimer >= comboWindowStart && stateTimer <= comboWindowEnd)
        {
            return true;
        }

        return false;
    }

    public void Attack(PlayerController player)
    {
        player.rb.AddForce(player.transform.right * player.transform.localScale.x * 5f, ForceMode2D.Impulse);

        ComboManager.ComboAdd(1);
    }
}
