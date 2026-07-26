using UnityEngine;

public class DifficultyManager : MonoBehaviour
{
    public static DifficultyManager Instance;

    [Header("Difficulty")]
    public float interval = 60f;
    public int difficultyLevel = 0;

    public float enemyDamageMultiplier = 1f;
    public float enemyAttackSpeedMultiplier = 1f;
    public float comboDecayMultiplier = 1f;


    private float timer;


    private void Awake()
    {
        Instance = this;
    }


    private void Update()
    {
        timer += Time.deltaTime;

        if (timer >= interval)
        {
            IncreaseDifficulty();
            timer = 0;
        }
    }


    private void IncreaseDifficulty()
    {
        difficultyLevel++;

        enemyDamageMultiplier += 0.15f;
        enemyAttackSpeedMultiplier += 0.10f;
        comboDecayMultiplier += 0.20f;


        PlayAdrenalineAnimation();
    }


    private void PlayAdrenalineAnimation()
    {
        if (DifficultyBannerUI.Instance != null)
        {
            DifficultyBannerUI.Instance.PlayBanner();
        }
    }
}