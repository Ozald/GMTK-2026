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


    public float timer;


    private void Awake()
    {
        Instance = this;
    }


    private void Update()
    {
        timer += Time.deltaTime;

        if (timer >= interval)
        {
            Debug.Log("Difficulty Increased");
            IncreaseDifficulty();
            timer = 0;
        }
    }


    private void IncreaseDifficulty()
    {
        difficultyLevel++;

        enemyDamageMultiplier += 1;
        enemyAttackSpeedMultiplier += 0.3f;
        comboDecayMultiplier += 0.5f;

        if (SpawnManager.Instance != null)
        {
            SpawnManager.Instance.IncreaseDifficulty();
        }

        PlayAdrenalineAnimation();
    }


    private void PlayAdrenalineAnimation()
    {
        if (DifficultyBannerUI.Instance != null)
            StartCoroutine(DifficultyBannerUI.Instance.PlayBanner());
    }
}