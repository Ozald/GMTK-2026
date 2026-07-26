using System.Collections;
using UnityEngine;

public class PauseTransition : MonoBehaviour
{
    public RectTransform panel;

    public ButtonTransition[] buttons;
    public AnimatedButton[] animatedButtons;

    public static bool isPauseOpen = false;

    public float offScreenDistance = 2000f;
    public float overshootAmount = 500f;

    private Vector2 velocity;

    public Vector2 centerPosition;
    public Vector2 targetPosition;

    private Coroutine settleCoroutine;

    [Header("Hide While Paused")]
    [SerializeField] private GameObject uiToHide;

    private void Start()
    {
        centerPosition = panel.anchoredPosition;

        // Start off the left side
        panel.anchoredPosition = centerPosition - new Vector2(offScreenDistance, 0);

        // Stay closed
        targetPosition = panel.anchoredPosition;
    }

    private void Update()
    {
        panel.anchoredPosition = Vector2.SmoothDamp(
            panel.anchoredPosition,
            targetPosition,
            ref velocity,
            0.15f,
            Mathf.Infinity,
            Time.unscaledDeltaTime
        );

        if (Input.GetKeyDown(KeyCode.P))
        {
            if (isPauseOpen)
            {
                Exit();
            }
            else
            {
                Enter();
            }
        }
    }

    public void Enter()
    {
        isPauseOpen = true;

        Time.timeScale = 0f;

        if (uiToHide != null)
            uiToHide.SetActive(false);

        targetPosition = centerPosition + new Vector2(overshootAmount, 0);

        foreach (ButtonTransition button in buttons)
        {
            button.Enter();
        }

        if (settleCoroutine != null)
            StopCoroutine(settleCoroutine);

        settleCoroutine = StartCoroutine(SettleAfterDelay());
    }

    private IEnumerator SettleAfterDelay()
    {
        yield return new WaitForSecondsRealtime(0.25f);
        Settle();
    }

    void Settle()
    {
        targetPosition = centerPosition;
    }

    public void Exit()
    {
        isPauseOpen = false;

        Time.timeScale = 1f;

        if (uiToHide != null)
            uiToHide.SetActive(true);

        if (settleCoroutine != null)
            StopCoroutine(settleCoroutine);

        targetPosition = centerPosition - new Vector2(offScreenDistance + overshootAmount, 0);

        foreach (ButtonTransition button in buttons)
        {
            button.Exit();
        }

        foreach (AnimatedButton button in animatedButtons)
        {
            button.ResetHighlight();
        }
    }
}