using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComboManager : MonoBehaviour
{
    #region Variables
    [Header("General")]
    public int currentCombo;
    public ComboGrade comboGrade;

    public enum ComboGrade
    {
        F, D, C, B, A, S, SS, SSS
    }

    [Header("GradeScale")]
    public int sssGrade = 450;
    public int ssGrade = 300;
    public int sGrade = 200;
    public int aGrade = 120;
    public int bGrade = 80;
    public int cGrade = 60;
    public int dGrade = 40;
    public int fGrade = 20;



    [Header("Intervals")]
    public float decayInterval = 1f;
    public float defaultDecayInterval = 1f;
    public float minimumDecayInterval = 0.2f;
    public float speedIncreaseRate = 0.1f;
    public float inactivityDelay = 5f;

    [Header("Timer")]
    public float decayTimer;
    public float inactivityTimer;

    #endregion

    #region General Function
    private void Update()
    {
        decayTimer += Time.deltaTime;
        inactivityTimer += Time.deltaTime;

        if (inactivityTimer >= inactivityDelay)
        {
            decayInterval -= speedIncreaseRate * Time.deltaTime;
            decayInterval = Mathf.Max(minimumDecayInterval, decayInterval);
        }

        if (decayTimer >= decayInterval)
        {
            decayTimer = 0f;
            ComboReduce(1);
        }
    }
    #endregion

    #region Combo Helpers
    public void ComboReduce(int amount)
    {
        currentCombo = Mathf.Max(0, currentCombo - amount);

        UpdateGrade();
    }

    public void ComboAdd(int amount)
    {
        currentCombo += amount;

        inactivityTimer = 0f;
        decayInterval = defaultDecayInterval;

        UpdateGrade();
    }

    private void UpdateGrade()
    {
        if (currentCombo >= sssGrade)
        {
            comboGrade = ComboGrade.SSS;
        }
        else if (currentCombo >= ssGrade)
        {
            comboGrade = ComboGrade.SS;
        }
        else if (currentCombo >= sGrade)
        {
            comboGrade = ComboGrade.S;
        }
        else if (currentCombo >= aGrade)
        {
            comboGrade = ComboGrade.A;
        }
        else if (currentCombo >= bGrade)
        {
            comboGrade = ComboGrade.B;
        }
        else if (currentCombo >= cGrade)
        {
            comboGrade = ComboGrade.C;
        }
        else if (currentCombo >= dGrade)
        {
            comboGrade = ComboGrade.D;
        }
        else if (currentCombo >= fGrade)
        {
            comboGrade = ComboGrade.F;
        }
    }
    #endregion

    #region Tick Down Rate Helpers
    public void SetRate(float amount)
    {
        decayInterval = amount;
    }

    public void RateIncrease(float amount)
    {
        decayInterval += amount;
    }

    public void RateDecrease(float amount)
    {
        decayInterval -= amount;
        decayInterval = Mathf.Max(minimumDecayInterval, decayInterval);
    }
    #endregion
}