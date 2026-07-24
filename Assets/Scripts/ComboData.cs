using UnityEngine;

[CreateAssetMenu(fileName = "ComboManager", menuName = "ScriptableObjects/Combo Manager", order = 1)]
public class ComboData : ScriptableObject
{
    [Header("Grade Scale")]
    public int sssGrade = 18000;
    public int ssGrade = 12000;
    public int sGrade = 8000;
    public int aGrade = 5000;
    public int bGrade = 3000;
    public int cGrade = 1500;
    public int dGrade = 500;
    public int fGrade = 0;


    [Header("Decay Settings")]
    public int decayAmount = 25;

    public float defaultDecayInterval = 0.5f;
    public float minimumDecayInterval = 0.15f;
    public float speedIncreaseRate = 0.1f;
    public float inactivityDelay = 3f;
}