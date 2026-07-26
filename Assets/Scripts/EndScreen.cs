using System.Collections;
using UnityEngine;
using TMPro;

public class EndScreen : MonoBehaviour
{
    [Header("Menu Movement")]
    [SerializeField] private RectTransform menuPanel;
    [SerializeField] private float menuDropDistance = 800f;
    [SerializeField] private float menuDropDuration = 0.5f;

    [Header("Text Slams")]
    [SerializeField] private RectTransform[] resultsTexts;
    [SerializeField] private float startScale = 1.8f;
    [SerializeField] private float fallDistance = 12f;
    [SerializeField] private float textDuration = 0.25f;
    [SerializeField] private float slamDelay = 0.15f;

    [Header("Result Text Values")]
    [SerializeField] private TMP_Text[] resultTextLabels;

    [Header("Move Results Left")]
    [SerializeField] private float moveLeftAmount = 100f;
    [SerializeField] private float moveLeftDuration = 0.4f;

    [Header("Final Text Delay")]
    [SerializeField] private float finalTextDelay = 0.5f;

    [Header("Final Text Slam")]
    [SerializeField] private RectTransform finalText;
    [SerializeField] private Vector2 finalStartPosition;
    [SerializeField] private float finalStartScale = 1.8f;
    [SerializeField] private float finalSlamDuration = 0.25f;


    private Vector2 menuOriginalPosition;

    private Vector2[] textOriginalPositions;
    private Vector3[] textOriginalScales;

    private Vector2 finalTextOriginalPosition;
    private Vector3 finalTextOriginalScale;


    private void Awake()
    {
        menuOriginalPosition = menuPanel.anchoredPosition;

        textOriginalPositions = new Vector2[resultsTexts.Length];
        textOriginalScales = new Vector3[resultsTexts.Length];

        for (int i = 0; i < resultsTexts.Length; i++)
        {
            textOriginalPositions[i] = resultsTexts[i].anchoredPosition;
            textOriginalScales[i] = resultsTexts[i].localScale;
        }

        finalTextOriginalPosition = finalText.anchoredPosition;
        finalTextOriginalScale = finalText.localScale;
    }


    private void Start()
    {
        LoadRunStats();

        foreach (RectTransform text in resultsTexts)
        {
            text.gameObject.SetActive(false);
        }

        finalText.gameObject.SetActive(false);

        StartCoroutine(PlayEntrance());
    }


    private void LoadRunStats()
    {
        if (RunStats.Instance == null)
        {
            Debug.LogWarning("No RunStats instance found!");
            return;
        }

        if (resultTextLabels.Length >= 5)
        {
            resultTextLabels[0].text = "Time Survived: " + FormatTime(RunStats.Instance.timeAlive);
            resultTextLabels[1].text = "Max Hit Streak: " + RunStats.Instance.highestHitCombo;
            resultTextLabels[2].text = "Total Combo Gained: " + RunStats.Instance.totalComboGain;

            int finalScore = RunStats.Instance.CalculateFinalScore();
            resultTextLabels[3].text = "Final Score: " + finalScore;

            resultTextLabels[4].text = RunStats.Instance.GetRank();
        }
        else
        {
            Debug.LogWarning("Need 5 result text labels assigned!");
        }
    }


    private string FormatTime(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60f);
        int seconds = Mathf.FloorToInt(time % 60f);

        return string.Format("{0:00}:{1:00}", minutes, seconds);
    }


    private IEnumerator PlayEntrance()
    {
        menuPanel.anchoredPosition = menuOriginalPosition + Vector2.up * menuDropDistance;

        float elapsed = 0f;

        while (elapsed < menuDropDuration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(elapsed / menuDropDuration);
            t = 1f - Mathf.Pow(1f - t, 3f);

            menuPanel.anchoredPosition = Vector2.Lerp(
                menuOriginalPosition + Vector2.up * menuDropDistance,
                menuOriginalPosition,
                t
            );

            yield return null;
        }

        menuPanel.anchoredPosition = menuOriginalPosition;

        yield return new WaitForSeconds(0.1f);

        StartCoroutine(SlamAllTexts());
    }


    private IEnumerator SlamAllTexts()
    {
        for (int i = 0; i < resultsTexts.Length; i++)
        {
            resultsTexts[i].gameObject.SetActive(true);

            StartCoroutine(SlamCoroutine(i));

            yield return new WaitForSeconds(slamDelay);
        }

        yield return new WaitForSeconds(0.2f);

        StartCoroutine(MoveResultsLeft());
    }


    private IEnumerator SlamCoroutine(int index)
    {
        RectTransform text = resultsTexts[index];

        Vector3 originalScale = textOriginalScales[index];
        Vector3 startScaleVector = originalScale * startScale;

        Vector2 endPos = textOriginalPositions[index];
        Vector2 startPos = endPos + Vector2.up * fallDistance;

        float elapsed = 0f;

        text.localScale = startScaleVector;
        text.anchoredPosition = startPos;

        while (elapsed < textDuration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(elapsed / textDuration);
            t = 1f - Mathf.Pow(1f - t, 3f);

            text.localScale = Vector3.Lerp(startScaleVector, originalScale, t);
            text.anchoredPosition = Vector2.Lerp(startPos, endPos, t);

            yield return null;
        }

        text.localScale = originalScale * 0.98f;
        yield return new WaitForSeconds(0.03f);

        text.localScale = originalScale * 1.01f;
        yield return new WaitForSeconds(0.03f);

        text.localScale = originalScale;
        text.anchoredPosition = endPos;
    }


    private IEnumerator MoveResultsLeft()
    {
        Vector2[] startPositions = new Vector2[resultsTexts.Length];

        for (int i = 0; i < resultsTexts.Length; i++)
        {
            startPositions[i] = resultsTexts[i].anchoredPosition;
        }

        float elapsed = 0f;

        while (elapsed < moveLeftDuration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(elapsed / moveLeftDuration);
            t = 1f - Mathf.Pow(1f - t, 3f);

            for (int i = 0; i < resultsTexts.Length; i++)
            {
                resultsTexts[i].anchoredPosition = Vector2.Lerp(
                    startPositions[i],
                    startPositions[i] + Vector2.left * moveLeftAmount,
                    t
                );
            }

            yield return null;
        }

        yield return new WaitForSeconds(finalTextDelay);

        StartCoroutine(FinalTextSlam());
    }


    private IEnumerator FinalTextSlam()
    {
        finalText.gameObject.SetActive(true);

        Vector3 startScale = finalTextOriginalScale * finalStartScale;

        finalText.localScale = startScale;
        finalText.anchoredPosition = finalStartPosition;

        float elapsed = 0f;

        while (elapsed < finalSlamDuration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(elapsed / finalSlamDuration);
            t = 1f - Mathf.Pow(1f - t, 3f);

            finalText.localScale = Vector3.Lerp(
                startScale,
                finalTextOriginalScale,
                t
            );

            finalText.anchoredPosition = Vector2.Lerp(
                finalStartPosition,
                finalTextOriginalPosition,
                t
            );

            yield return null;
        }

        finalText.localScale = finalTextOriginalScale;
        finalText.anchoredPosition = finalTextOriginalPosition;
    }
}