using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "UppercutData", menuName = "ScriptableObjects/Player Attacks/Uppercut Data", order = 1)]
public class UppercutData : AttackData
{
    public override void Attack(PlayerController player)
    {
        Debug.Log("Uppercut Attack");
        player.rb.AddForce(player.transform.right * player.transform.localScale.x * playerKnockback, ForceMode2D.Impulse);
    }
}
