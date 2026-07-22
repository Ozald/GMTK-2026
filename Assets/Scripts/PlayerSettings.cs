using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerSettings", menuName = "ScriptableObjects/Player Settings", order = 1)]
public class PlayerSettings : ScriptableObject
{
    public float verticalSpeed = 5f;
    public float horizontalSpeed = 5f;
}
