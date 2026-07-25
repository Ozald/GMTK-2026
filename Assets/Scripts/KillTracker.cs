using System.Collections;
using UnityEngine;

public class KillTracker : MonoBehaviour
{
    public static KillTracker Instance;

    [SerializeField] private float multiKillWindow = 1f;

    [Header("Kill Multipliers")]
    public float doubleKillMultiplier = 1.25f;
    public float tripleKillMultiplier = 1.5f;
    public float massacreMultiplier = 2f;
    public float maxKillMultiplier = 3f;

    private int killCount = 0;
    private Coroutine resetCoroutine;

    private float currentKillMultiplier = 1f;


    private void Awake()
    {
        Instance = this;
    }


    public void EnemyKilled()
    {
        killCount++;

        if (resetCoroutine != null)
        {
            StopCoroutine(resetCoroutine);
        }

        resetCoroutine = StartCoroutine(CheckMultiKill());
    }


    private IEnumerator CheckMultiKill()
    {
        yield return new WaitForSeconds(multiKillWindow);


        if (killCount >= 2)
        {
            currentKillMultiplier = GetKillMultiplier(killCount);

            CombatFeed.Instance.Add(GetKillMessage(killCount));
        }
        else
        {
            currentKillMultiplier = 1f;
        }


        killCount = 0;
    }


    private float GetKillMultiplier(int kills)
    {
        switch (kills)
        {
            case 2:
                return doubleKillMultiplier;

            case 3:
                return tripleKillMultiplier;

            case 4:
                return massacreMultiplier;

            default:
                return Mathf.Min(
                    1f + (kills * 0.25f),
                    maxKillMultiplier
                );
        }
    }


    public float GetKillMultiplier()
    {
        return currentKillMultiplier;
    }


    private string GetKillMessage(int kills)
    {
        switch (kills)
        {
            case 2:
                return "Double Kill!";

            case 3:
                return "Triple Kill!";

            case 4:
                return "Massacre!";

            default:
                return $"MultiKill (x{kills})";
        }
    }
}