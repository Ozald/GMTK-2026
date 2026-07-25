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
}