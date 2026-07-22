using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ComboManager", menuName = "ScriptableObjects/Combo Manager", order = 1)]

public class ComboData : ScriptableObject
{
    [Header("Grade Scale")]
    public int sssGrade = 450;
    public int ssGrade = 300;
    public int sGrade = 200;
    public int aGrade = 120;
    public int bGrade = 80;
    public int cGrade = 60;
    public int dGrade = 40;
    public int fGrade = 20;


    [Header("Decay Settings")]
    public float defaultDecayInterval = 1f;
    public float minimumDecayInterval = 0.2f;
    public float speedIncreaseRate = 0.1f;
    public float inactivityDelay = 5f;
}
