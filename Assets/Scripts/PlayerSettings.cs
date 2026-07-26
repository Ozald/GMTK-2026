using FMODUnity;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerSettings", menuName = "ScriptableObjects/Player Settings", order = 1)]
public class PlayerSettings : ScriptableObject
{
    public float verticalSpeed = 5f;
    public float horizontalSpeed = 5f;
    public float dashForce = 5f;
    public float dashDuration = 0.5f;
    public float stunDuration = 0.3f;

    [Header("Parry Settings")]
    public float parryDuration = 0.75f;
    public float parryKnockback = 100f;
    public float parryWindow = 0.4f;

    public LayerMask enemyAttackLayer;

    [Header("Attacks")]
    [InspectorName("Z Key")] public AttackData punchAttack;

    [Header("Audio")]
    public EventReference dashSound;
}
