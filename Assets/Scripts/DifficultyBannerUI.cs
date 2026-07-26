using System.Collections;
using UnityEngine;

public class DifficultyBannerUI : MonoBehaviour
{
    [SerializeField] private RectTransform banner;

    [Header("Positions")]
    [SerializeField] private float startX = -800f;   // starts off left
    [SerializeField] private float slowDownX = -100f; // point where it begins slowing
    [SerializeField] private float centerX = 0f;      // final center position
    [SerializeField] private float endX = 800f;       // leaves right
    [SerializeField] private float yPosition = 0f;    // banner height

    public static DifficultyBannerUI Instance;

    [Header("Movement")]
    [SerializeField] private float fastSpeed = 800f;
    [SerializeField] private float slowSpeed = 50f;
    [SerializeField] private float exitSpeed = 800f;

    [Header("Timing")]
    [SerializeField] private float centerTime = 2f;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        banner.anchoredPosition = new Vector2(startX, yPosition);

        //StartCoroutine(PlayBanner());
    }

    public IEnumerator PlayBanner()
    {
        // Move fast until slow down point
        while (banner.anchoredPosition.x < slowDownX)
        {
            float x = banner.anchoredPosition.x + fastSpeed * Time.deltaTime;

            banner.anchoredPosition = new Vector2(x, yPosition);

            yield return null;
        }


        // Slowly move into center
        while (banner.anchoredPosition.x < centerX)
        {
            float x = banner.anchoredPosition.x + slowSpeed * Time.deltaTime;

            banner.anchoredPosition = new Vector2(
                Mathf.Min(x, centerX),
                yPosition
            );

            yield return null;
        }


        // Slowly drift while staying near center
        float timer = 0;

        while (timer < centerTime)
        {
            timer += Time.deltaTime;

            float x = banner.anchoredPosition.x + slowSpeed * Time.deltaTime;

            banner.anchoredPosition = new Vector2(x, yPosition);

            yield return null;
        }


        // Exit right
        while (banner.anchoredPosition.x < endX)
        {
            float x = banner.anchoredPosition.x + exitSpeed * Time.deltaTime;

            banner.anchoredPosition = new Vector2(x, yPosition);

            yield return null;
        }


        banner.anchoredPosition = new Vector2(endX, yPosition);
    }
}