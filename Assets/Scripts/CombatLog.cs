using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CombatFeed : MonoBehaviour
{
    public static CombatFeed Instance;

    [SerializeField] private Transform feedContainer;

    [SerializeField] private TMP_Text[] feedEntries;

    [SerializeField] public TMP_Text hitCounterText;
    [SerializeField] public TMP_Text hitRankText;

    [SerializeField] private int maxEntries = 5;
    private Coroutine[] entryAnimations;

    private List<string> entries = new();

    private int hitChain = 0;
    private float lastHitTime = -100f;

    [SerializeField] private float hitChainTime = 10f;

    [SerializeField] private float pulseScale = 1.25f;
    [SerializeField] private float pulseDuration = 0.15f;

    [SerializeField] private float rankSlamScale = 1.5f;
    [SerializeField] private float rankSlamDuration = 0.25f;

    private Vector3 rankOriginalScale;
    private Color rankOriginalColor;
    private Coroutine rankAnimationCoroutine;
    private string lastRank = "";

    private Vector3 originalScale;
    private Coroutine pulseCoroutine;


    private void Awake()
    {
        Instance = this;

        entryAnimations = new Coroutine[feedEntries.Length];

        originalScale = hitCounterText.transform.localScale;

        rankOriginalScale = hitRankText.transform.localScale;
        rankOriginalColor = hitRankText.color;
    }


    private void Update()
    {
        // Reset combo after time expires
        if (hitChain > 0 && Time.time - lastHitTime > hitChainTime)
        {
            hitChain = 0;
            UpdateHitCounter();
        }
    }


    public void Add(string message)
    {
        // Move old messages down
        for (int i = feedEntries.Length - 1; i > 0; i--)
        {
            // Stop animation on the entry being moved
            if (entryAnimations[i] != null)
            {
                StopCoroutine(entryAnimations[i]);
                entryAnimations[i] = null;
            }

            feedEntries[i].text = feedEntries[i - 1].text;

            // Force scale back to normal
            feedEntries[i].transform.localScale = Vector3.one;
        }


        // Stop animation on the newest entry
        if (entryAnimations[0] != null)
        {
            StopCoroutine(entryAnimations[0]);
            entryAnimations[0] = null;
        }


        // Add new message
        feedEntries[0].text = message;

        // Make sure it starts normal
        feedEntries[0].transform.localScale = Vector3.one;


        // Animate only the new entry
        entryAnimations[0] = StartCoroutine(SlamEntry(feedEntries[0]));
    }

    private IEnumerator SlamEntry(TMP_Text entry)
    {
        Vector3 originalScale = Vector3.one;

        entry.transform.localScale = Vector3.zero;

        float timer = 0f;
        float duration = 0.15f;
        float maxScale = 1.3f;


        while (timer < duration)
        {
            timer += Time.unscaledDeltaTime;

            float t = Mathf.SmoothStep(0, 1, timer / duration);

            entry.transform.localScale = Vector3.Lerp(
                Vector3.zero,
                originalScale * maxScale,
                t
            );

            yield return null;
        }


        timer = 0f;

        while (timer < 0.1f)
        {
            timer += Time.unscaledDeltaTime;

            float t = Mathf.SmoothStep(0, 1, timer / 0.1f);

            entry.transform.localScale = Vector3.Lerp(
                originalScale * maxScale,
                originalScale,
                t
            );

            yield return null;
        }


        entry.transform.localScale = originalScale;
    }


    public void AddHit()
    {
        if (Time.time - lastHitTime > hitChainTime)
        {
            hitChain = 1;
        }
        else
        {
            hitChain++;
        }

        lastHitTime = Time.time;

        UpdateHitCounter();

        if (pulseCoroutine != null)
            StopCoroutine(pulseCoroutine);

        pulseCoroutine = StartCoroutine(PulseHitCounter());
    }


    public int GetHitChain()
    {
        return hitChain;
    }

    public void ResetHitChain()
    {
        hitChain = 0;
        lastHitTime = -100f;
        UpdateHitCounter();
    }

    private void UpdateHitCounter()
    {
        if (hitChain <= 0)
        {
            hitCounterText.text = "";
            hitRankText.text = "";
            lastRank = "";
        }
        else
        {
            hitCounterText.text = $"x{hitChain} Hits";

            string newRank = GetHitRank(hitChain);

            hitRankText.text = newRank;


            // Only slam if the rank actually changed
            if (newRank != lastRank && newRank != "")
            {
                if (rankAnimationCoroutine != null)
                    StopCoroutine(rankAnimationCoroutine);

                rankAnimationCoroutine = StartCoroutine(RankSlam());
            }

            lastRank = newRank;
        }
    }

    private string GetHitRank(int hits)
    {
        if (hits >= 50)
            return "Amazing";

        if (hits >= 40)
            return "Radical";

        if (hits >= 30)
            return "Awesome";

        if (hits >= 20)
            return "Sick";

        if (hits >= 10)
            return "Great";

        if (hits >= 5)
            return "Good";

        return "";
    }

    private IEnumerator PulseHitCounter()
    {
        float timer = 0f;

        Vector3 startScale = originalScale;
        Vector3 peakScale = originalScale * pulseScale;

        // Grow
        while (timer < pulseDuration)
        {
            timer += Time.unscaledDeltaTime;

            float t = Mathf.SmoothStep(0, 1, timer / pulseDuration);

            hitCounterText.transform.localScale = Vector3.Lerp(
                startScale,
                peakScale,
                t
            );

            yield return null;
        }


        timer = 0f;

        // Return to normal
        while (timer < pulseDuration)
        {
            timer += Time.unscaledDeltaTime;

            float t = Mathf.SmoothStep(0, 1, timer / pulseDuration);

            hitCounterText.transform.localScale = Vector3.Lerp(
                peakScale,
                startScale,
                t
            );

            yield return null;
        }


        hitCounterText.transform.localScale = originalScale;
    }

    private IEnumerator RankSlam()
    {
        float timer = 0f;

        hitRankText.gameObject.SetActive(true);

        Color startColor = rankOriginalColor;
        startColor.a = 0;

        hitRankText.color = startColor;
        hitRankText.transform.localScale = rankOriginalScale * 0.2f;


        // Slam in
        while (timer < rankSlamDuration)
        {
            timer += Time.unscaledDeltaTime;

            float t = Mathf.SmoothStep(0, 1, timer / rankSlamDuration);

            hitRankText.transform.localScale = Vector3.Lerp(
                Vector3.zero,
                rankOriginalScale * rankSlamScale,
                t
            );

            hitRankText.color = Color.Lerp(
                startColor,
                rankOriginalColor,
                t
            );

            yield return null;
        }


        // Settle back down
        timer = 0f;

        while (timer < 0.1f)
        {
            timer += Time.unscaledDeltaTime;

            float t = Mathf.SmoothStep(0, 1, timer / 0.1f);

            hitRankText.transform.localScale = Vector3.Lerp(
                rankOriginalScale * rankSlamScale,
                rankOriginalScale,
                t
            );

            yield return null;
        }


        hitRankText.transform.localScale = rankOriginalScale;
    }
}