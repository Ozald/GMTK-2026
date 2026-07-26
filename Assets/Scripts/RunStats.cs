using UnityEngine;
using TMPro;

public class RunStats : MonoBehaviour
{
    public static RunStats Instance { get; private set; }


    [Header("Results Stats")]
    public float timeAlive;
    public int highestHitCombo;
    public int totalComboGain;


    [Header("Final Score")]
    public int finalScore;

    public bool isTrackingTime;


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }


    private void Update()
    {
        if (isTrackingTime)
        {
            timeAlive += Time.deltaTime;
        }
    }


    public void StartTimer()
    {
        isTrackingTime = true;
    }


    public void StopTimer()
    {
        isTrackingTime = false;
    }


    public void UpdateHighestHitCombo(int hitCombo)
    {
        if (hitCombo > highestHitCombo)
        {
            highestHitCombo = hitCombo;
        }
    }

    public string GetRank()
    {
        int score = CalculateFinalScore();

        if (score >= 100000)
            return "S";

        if (score >= 60000)
            return "A";

        if (score >= 40000)
            return "B";

        if (score >= 20000)
            return "C";

        return "D";
    }

    public void AddComboGain(int amount)
    {
        totalComboGain += amount;
    }


    public void ResetStats()
    {
        timeAlive = 0;
        highestHitCombo = 0;
        totalComboGain = 0;
        isTrackingTime = false;
    }


    public int CalculateFinalScore()
    {
        finalScore =
            Mathf.RoundToInt(timeAlive * 10)
            + (highestHitCombo * 50)
            + (totalComboGain * 5);

        return finalScore;
    }
}