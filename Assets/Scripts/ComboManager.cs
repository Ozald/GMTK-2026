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

    [SerializeField] private float multiplierIncrease = 0.1f;
    [SerializeField] private float maxMultiplier = 3f;


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


    [Header("Rank Decay Multipliers")]
    public float fDecayMultiplier = 1f;
    public float dDecayMultiplier = 1.25f;
    public float cDecayMultiplier = 1.5f;
    public float bDecayMultiplier = 2f;
    public float aDecayMultiplier = 3f;
    public float sDecayMultiplier = 4f;
    public float ssDecayMultiplier = 5f;
    public float sssDecayMultiplier = 6f;


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

        currentCombo = (comboData.cGrade + comboData.bGrade) / 2;

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


        if (rankUpImmunityTimer <= 0)
        {
            if (decayTimer >= GetCurrentDecayInterval())
            {
                decayTimer = 0f;

                ComboReduce(comboData.decayAmount);
            }
        }
    }

    #endregion



    #region Combo Helpers


    public static void ComboAdd(int amount)
    {
        Instance.currentCombo += amount;

        if (RunStats.Instance != null)
        {
            RunStats.Instance.AddComboGain(amount);
        }

        // Attacking keeps your combo alive
        Instance.decayTimer = 0f;

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

    public static void TakeDamage(int amount)
    {
        Instance.currentCombo = Mathf.Max(
            0,
            Instance.currentCombo - amount
        );

        Instance.decayTimer = 0f;

        // Player got hit, reset hit chain
        if (CombatFeed.Instance != null)
        {
            CombatFeed.Instance.ResetHitChain();
        }

        Instance.CheckGradeChange();
    }



    private void CheckGradeChange()
    {
        ComboGrade newGrade = CalculateGrade();


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


        yield return new WaitForSeconds(rankUpDelay);


        displayGrade = pendingGrade;
        comboGrade = pendingGrade;


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
        }

        return 0;
    }


    #endregion



    #region Decay Helpers


    private float GetCurrentDecayInterval()
    {
        float multiplier = 1f;


        switch (displayGrade)
        {
            case ComboGrade.F:
                multiplier = fDecayMultiplier;
                break;

            case ComboGrade.D:
                multiplier = dDecayMultiplier;
                break;

            case ComboGrade.C:
                multiplier = cDecayMultiplier;
                break;

            case ComboGrade.B:
                multiplier = bDecayMultiplier;
                break;

            case ComboGrade.A:
                multiplier = aDecayMultiplier;
                break;

            case ComboGrade.S:
                multiplier = sDecayMultiplier;
                break;

            case ComboGrade.SS:
                multiplier = ssDecayMultiplier;
                break;

            case ComboGrade.SSS:
                multiplier = sssDecayMultiplier;
                break;
        }


        return Mathf.Max(
            comboData.minimumDecayInterval,
            decayInterval / multiplier
        );
    }



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

    public static float GetDamageMultiplier()
    {
        float multiplier = 1f + (Instance.currentCombo * Instance.multiplierIncrease);

        return Mathf.Min(multiplier, Instance.maxMultiplier);
    }

    #endregion
}