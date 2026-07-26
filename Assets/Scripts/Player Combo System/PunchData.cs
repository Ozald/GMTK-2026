using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PunchData", menuName = "ScriptableObjects/Player Attacks/Punch Data", order = 1)]
public class PunchData : AttackData
{
    public override void Attack(PlayerController player)
    {
        Debug.Log("Punch Attack");
        player.rb.AddForce(player.transform.right * player.transform.localScale.x * playerKnockback, ForceMode2D.Impulse);
    }
}
