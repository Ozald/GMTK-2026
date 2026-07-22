using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

public class ComboBarUI : MonoBehaviour
{
    public Image radialIndicatorUI;
    public TextMeshProUGUI comboText;
    public ComboManager comboManager;


    // Start is called before the first frame update
    void Start()
    {
        comboManager = FindObjectOfType<ComboManager>();
    }

    // Update is called once per frame
    void Update()
    {
        radialIndicatorUI.fillAmount = (float)(comboManager.GetComboProgress() * 0.75);
        comboText.text = comboManager.comboGrade.ToString();
    }
}



