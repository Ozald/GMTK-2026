using UnityEngine;

public class ComboManager : MonoBehaviour
{
    #region Variables

    [Header("Data")]
    public ComboData comboData;


    [Header("Runtime")]
    public int currentCombo;
    public ComboGrade comboGrade;


    public enum ComboGrade
    {
        None,
        F,
        D,
        C,
        B,
        A,
        S,
        SS,
        SSS
    }


    [Header("Timers")]
    public float decayTimer;
    public float inactivityTimer;

    public float decayInterval;

    #endregion

    #region General
    private void Start()
    {
        decayInterval = comboData.defaultDecayInterval;

        UpdateGrade();
    }

    private void Update()
    {
        decayTimer += Time.deltaTime;
        inactivityTimer += Time.deltaTime;


        if (inactivityTimer >= comboData.inactivityDelay)
        {
            decayInterval -= comboData.speedIncreaseRate * Time.deltaTime;

            decayInterval = Mathf.Max(comboData.minimumDecayInterval,decayInterval);
        }


        if (decayTimer >= decayInterval)
        {
            decayTimer = 0f;

            ComboReduce(1);
        }
    }
    #endregion

    #region Combo Helpers
    public void ComboAdd(int amount)
    {
        currentCombo += amount;

        inactivityTimer = 0f;
        decayInterval = comboData.defaultDecayInterval;

        UpdateGrade();
    }

    public void ComboReduce(int amount)
    {
        currentCombo = Mathf.Max(0,currentCombo - amount);


        UpdateGrade();
    }

    private void UpdateGrade()
    {
        if (currentCombo >= comboData.sssGrade)
            comboGrade = ComboGrade.SSS;

        else if (currentCombo >= comboData.ssGrade)
            comboGrade = ComboGrade.SS;

        else if (currentCombo >= comboData.sGrade)
            comboGrade = ComboGrade.S;

        else if (currentCombo >= comboData.aGrade)
            comboGrade = ComboGrade.A;

        else if (currentCombo >= comboData.bGrade)
            comboGrade = ComboGrade.B;

        else if (currentCombo >= comboData.cGrade)
            comboGrade = ComboGrade.C;

        else if (currentCombo >= comboData.dGrade)
            comboGrade = ComboGrade.D;

        else if (currentCombo >= comboData.fGrade)
            comboGrade = ComboGrade.F;

        else
            comboGrade = ComboGrade.None;
    }
    #endregion

    #region Decay Helpers
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

        decayInterval = Mathf.Max(
            comboData.minimumDecayInterval,
            decayInterval
        );
    }
    #endregion
}