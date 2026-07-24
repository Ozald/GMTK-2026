using System.Collections;
using UnityEngine;

public class ComboManager : MonoBehaviour
{
    #region Variables

    public static ComboManager Instance { get; private set; }

    [Header("Data")]
    public ComboData comboData;


    [Header("Runtime")]
    public int currentCombo;
    public ComboGrade comboGrade;
    public ComboGrade displayGrade;


    public enum ComboGrade
    {
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


    [Header("Rank Up")]
    public float rankUpDelay = 0.5f;
    public float rankUpImmunityDuration = 2f;

    private float rankUpImmunityTimer;
    private bool isRankingUp;

    private ComboGrade pendingGrade;

    #endregion


    #region General

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
    }


    private void Start()
    {
        decayInterval = comboData.defaultDecayInterval;

        comboGrade = CalculateGrade();
        displayGrade = comboGrade;
    }


    private void Update()
    {
        decayTimer += Time.deltaTime;
        inactivityTimer += Time.deltaTime;


        if (rankUpImmunityTimer > 0)
        {
            rankUpImmunityTimer -= Time.deltaTime;
        }


        if (inactivityTimer >= comboData.inactivityDelay)
        {
            decayInterval -= comboData.speedIncreaseRate * Time.deltaTime;

            decayInterval = Mathf.Max(
                comboData.minimumDecayInterval,
                decayInterval
            );
        }


        // Stop decay during rank immunity
        if (rankUpImmunityTimer <= 0)
        {
            if (decayTimer >= decayInterval)
            {
                decayTimer = 0f;

                ComboReduce(1);
            }
        }
    }

    #endregion



    #region Combo Helpers


    public static void ComboAdd(int amount)
    {
        Instance.currentCombo += amount;

        Instance.inactivityTimer = 0f;
        Instance.decayInterval = Instance.comboData.defaultDecayInterval;

        Instance.CheckGradeChange();
    }


    public static void ComboReduce(int amount)
    {
        Instance.currentCombo = Mathf.Max(
            0,
            Instance.currentCombo - amount
        );

        Instance.CheckGradeChange();
    }



    private void CheckGradeChange()
    {
        ComboGrade newGrade = CalculateGrade();


        // Rank up
        if (newGrade > displayGrade)
        {
            pendingGrade = newGrade;

            if (!isRankingUp)
                StartCoroutine(RankUpSequence());
        }
        else
        {
            comboGrade = newGrade;
            displayGrade = newGrade;
        }
    }



    private IEnumerator RankUpSequence()
    {
        isRankingUp = true;


        // Your UI can animate the bar to full here
        // while waiting

        yield return new WaitForSeconds(rankUpDelay);



        // Apply new rank
        displayGrade = pendingGrade;
        comboGrade = pendingGrade;


        // Give immunity
        rankUpImmunityTimer = rankUpImmunityDuration;


        isRankingUp = false;
    }



    public float GetComboProgress()
    {
        ComboGrade nextGrade = GetNextGrade(displayGrade);


        int nextRequirement = GetGradeRequirement(nextGrade);
        int currentRequirement = GetGradeRequirement(displayGrade);


        if (nextRequirement <= currentRequirement)
            return 1f;


        return Mathf.Clamp01(
            (float)(currentCombo - currentRequirement) /
            (nextRequirement - currentRequirement)
        );
    }


    #endregion



    #region Grade Helpers


    private ComboGrade CalculateGrade()
    {
        if (currentCombo >= comboData.sssGrade)
            return ComboGrade.SSS;

        else if (currentCombo >= comboData.ssGrade)
            return ComboGrade.SS;

        else if (currentCombo >= comboData.sGrade)
            return ComboGrade.S;

        else if (currentCombo >= comboData.aGrade)
            return ComboGrade.A;

        else if (currentCombo >= comboData.bGrade)
            return ComboGrade.B;

        else if (currentCombo >= comboData.cGrade)
            return ComboGrade.C;

        else if (currentCombo >= comboData.dGrade)
            return ComboGrade.D;

        return ComboGrade.F;
    }



    public ComboGrade GetNextGrade(ComboGrade grade)
    {
        int nextIndex = (int)grade + 1;


        if (nextIndex >= System.Enum.GetValues(typeof(ComboGrade)).Length)
            return grade;


        return (ComboGrade)nextIndex;
    }



    public int GetComboNeededForNextGrade()
    {
        switch (GetNextGrade(displayGrade))
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