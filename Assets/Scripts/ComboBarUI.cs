using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ComboBarUI : MonoBehaviour
{
    public Image radialIndicatorUI;

    public TextMeshProUGUI comboGradeText;
    public TextMeshProUGUI comboNumberText;

    public ComboManager comboManager;

    public float fillSpeed = 5f;

    private float targetFillAmount;

    [SerializeField] private float gradeSlamScale = 1.5f;
    [SerializeField] private float gradeSlamDuration = 0.25f;

    [SerializeField] private float rainbowSpeed = 2f;

    private Vector3 gradeOriginalScale;
    private Coroutine gradeSlamCoroutine;

    private ComboManager.ComboGrade lastDisplayedGrade;


    void Start()
    {
        comboManager = FindObjectOfType<ComboManager>();

        gradeOriginalScale = comboGradeText.transform.localScale;

        lastDisplayedGrade = comboManager.comboGrade;
    }

    void Update()
    {
        targetFillAmount = comboManager.GetComboProgress();

        radialIndicatorUI.fillAmount = Mathf.Lerp(
            radialIndicatorUI.fillAmount,
            targetFillAmount,
            Time.deltaTime * fillSpeed
        );


        comboNumberText.text = comboManager.currentCombo.ToString();

        if (comboManager.comboGrade != lastDisplayedGrade)
        {
            if (comboManager.comboGrade > lastDisplayedGrade)
            {
                if (gradeSlamCoroutine != null)
                    StopCoroutine(gradeSlamCoroutine);

                gradeSlamCoroutine = StartCoroutine(GradeSlam());
            }

            lastDisplayedGrade = comboManager.comboGrade;
        }

        comboGradeText.text = comboManager.comboGrade.ToString();

        if (comboManager.comboGrade == ComboManager.ComboGrade.SSS)
        {
            comboGradeText.color = GetRainbowColor();
        }
        else
        {
            comboGradeText.color = GetGradeColor(comboManager.comboGrade);
        }


        //if (CombatFeed.Instance != null)
        //{
        //    hitNumberText.text = CombatFeed.Instance.GetHitChain().ToString();
        //}
        //else
        //{
        //    hitNumberText.text = "x" + CombatFeed.Instance.GetHitChain() + " Hits";
        //}
    }

    private IEnumerator GradeSlam()
    {
        float timer = 0f;

        comboGradeText.transform.localScale = Vector3.zero;


        while (timer < gradeSlamDuration)
        {
            timer += Time.unscaledDeltaTime;

            float t = Mathf.SmoothStep(
                0,
                1,
                timer / gradeSlamDuration
            );

            comboGradeText.transform.localScale = Vector3.Lerp(
                Vector3.zero,
                gradeOriginalScale * gradeSlamScale,
                t
            );

            yield return null;
        }


        timer = 0f;

        float settleDuration = 0.1f;

        while (timer < settleDuration)
        {
            timer += Time.unscaledDeltaTime;

            float t = Mathf.SmoothStep(
                0,
                1,
                timer / settleDuration
            );

            comboGradeText.transform.localScale = Vector3.Lerp(
                gradeOriginalScale * gradeSlamScale,
                gradeOriginalScale,
                t
            );

            yield return null;
        }


        comboGradeText.transform.localScale = gradeOriginalScale;
    }

    private Color GetGradeColor(ComboManager.ComboGrade grade)
    {
        switch (grade)
        {
            case ComboManager.ComboGrade.F:
                return Color.magenta;

            case ComboManager.ComboGrade.D:
                return Color.cyan;

            case ComboManager.ComboGrade.C:
                return Color.green;

            case ComboManager.ComboGrade.B:
                return Color.yellow;

            case ComboManager.ComboGrade.A:
                return Color.red;

            case ComboManager.ComboGrade.S:
                return new Color(1f, 0.84f, 0.1f);

            case ComboManager.ComboGrade.SS:
                return new Color(0.7f, 0.9f, 1f);

            case ComboManager.ComboGrade.SSS:
                return new Color(1f, 0.84f, 0.1f);
        }

        return Color.white;
    }

    private Color GetRainbowColor()
    {
        float hue = Mathf.Repeat(Time.time * rainbowSpeed, 1f);

        return Color.HSVToRGB(
            hue,
            0.8f,
            1f
        );
    }
}