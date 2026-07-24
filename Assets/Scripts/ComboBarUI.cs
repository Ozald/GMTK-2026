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

    void Start()
    {
        comboManager = FindObjectOfType<ComboManager>();
    }

    void Update()
    {
        targetFillAmount = (float)comboManager.GetComboProgress();

        radialIndicatorUI.fillAmount = Mathf.Lerp(
            radialIndicatorUI.fillAmount,
            targetFillAmount,
            Time.deltaTime * fillSpeed
        );

        comboNumberText.text = comboManager.currentCombo.ToString();
        comboGradeText.text = comboManager.comboGrade.ToString();
    }
}