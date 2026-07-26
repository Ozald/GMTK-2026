using System.Collections;
using UnityEngine;

public class ResultsScreen : MonoBehaviour
{
    [SerializeField] private RectTransform resultsText;

    private Vector2 originalPosition;

    private void Awake()
    {
        originalPosition = resultsText.anchoredPosition;
    }

    private void Start()
    {
        Debug.Log("ResultsScreen started!");
        PlaySlam();
    }

    public void PlaySlam()
    {
        StopAllCoroutines();
        StartCoroutine(SlamCoroutine());
    }

    private IEnumerator SlamCoroutine()
    {
        Vector3 startScale = Vector3.one * 2.2f;
        Vector3 endScale = Vector3.one;

        Vector2 endPos = originalPosition;
        Vector2 startPos = endPos + Vector2.up * 40;

        float duration = 0.12f;
        float elapsed = 0f;

        resultsText.localScale = startScale;
        resultsText.anchoredPosition = startPos;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            resultsText.localScale = Vector3.Lerp(startScale, endScale, t);
            resultsText.anchoredPosition = Vector2.Lerp(startPos, endPos, t);

            yield return null;
        }

        // Small impact bounce
        resultsText.localScale = Vector3.one * 0.9f;
        yield return new WaitForSeconds(0.03f);

        resultsText.localScale = Vector3.one * 1.05f;
        yield return new WaitForSeconds(0.03f);

        resultsText.localScale = Vector3.one;
        resultsText.anchoredPosition = endPos;
    }
}