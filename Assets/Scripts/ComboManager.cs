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

    public float GetComboProgress()
    {
        ComboGrade nextGrade = GetNextGrade();

        int nextRequirement = GetGradeRequirement(nextGrade);
        int currentRequirement = GetGradeRequirement(comboGrade);

        if (nextRequirement <= currentRequirement)
            return 1f;

        return Mathf.Clamp01(
            (float)(currentCombo - currentRequirement) /
            (nextRequirement - currentRequirement)
        );

    }
    #endregion

    #region Grade Helpers
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

    public ComboGrade GetNextGrade()
    {
        int nextIndex = (int)comboGrade + 1;

        if (nextIndex >= System.Enum.GetValues(typeof(ComboGrade)).Length)
            return comboGrade;

        return (ComboGrade)nextIndex;
    }

    public int GetComboNeededForNextGrade()
    {
        switch (GetNextGrade())
        {
            case ComboGrade.F:
                return Mathf.Max(0, comboData.fGrade - currentCombo);

            case ComboGrade.D:
                return Mathf.Max(0, comboData.dGrade - currentCombo);

            case ComboGrade.C:
                return Mathf.Max(0, comboData.cGrade - currentCombo);

            case ComboGrade.B:
                return Mathf.Max(0, comboData.bGrade - currentCombo);

            case ComboGrade.A:
                return Mathf.Max(0, comboData.aGrade - currentCombo);

            case ComboGrade.S:
                return Mathf.Max(0, comboData.sGrade - currentCombo);

            case ComboGrade.SS:
                return Mathf.Max(0, comboData.ssGrade - currentCombo);

            case ComboGrade.SSS:
                return Mathf.Max(0, comboData.sssGrade - currentCombo);

            default:
                return 0;
        }
    }
    public int GetGradeRequirement(ComboGrade grade)
    {
        switch (grade)
        {
            case ComboGrade.F:
                return comboData.fGrade;

            case ComboGrade.D:
                return comboData.dGrade;

            case ComboGrade.C:
                return comboData.cGrade;

            case ComboGrade.B:
                return comboData.bGrade;

            case ComboGrade.A:
                return comboData.aGrade;

            case ComboGrade.S:
                return comboData.sGrade;

            case ComboGrade.SS:
                return comboData.ssGrade;

            case ComboGrade.SSS:
                return comboData.sssGrade;

            default:
                return 0;
        }
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