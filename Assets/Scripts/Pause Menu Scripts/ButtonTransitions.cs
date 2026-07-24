using System.Collections;
using UnityEngine;

public class ButtonTransition : MonoBehaviour
{
    public RectTransform button;

    public float startOffset = 500f;
    public float overshootAmount = 50f;
    public float delay = 0f;

    private Vector2 startPosition;
    private Vector2 targetPosition;

    private Vector2 velocity;

    private Coroutine enterCoroutine;
    private Coroutine settleCoroutine;

    void Start()
    {
        startPosition = button.anchoredPosition;

        // Put it off-screen
        button.anchoredPosition = startPosition - new Vector2(startOffset, 0);

        // Stay off-screen until Enter() is called
        targetPosition = button.anchoredPosition;
    }

    public void Enter()
    {
        if (enterCoroutine != null)
            StopCoroutine(enterCoroutine);

        if (settleCoroutine != null)
            StopCoroutine(settleCoroutine);

        enterCoroutine = StartCoroutine(EnterRoutine());
    }

    private IEnumerator EnterRoutine()
    {
        yield return new WaitForSecondsRealtime(delay);

        targetPosition = startPosition + new Vector2(overshootAmount, 0);

        settleCoroutine = StartCoroutine(SettleRoutine());
    }

    private IEnumerator SettleRoutine()
    {
        yield return new WaitForSecondsRealtime(0.15f);

        targetPosition = startPosition;
    }

    public void Exit()
    {
        if (enterCoroutine != null)
            StopCoroutine(enterCoroutine);

        if (settleCoroutine != null)
            StopCoroutine(settleCoroutine);

        targetPosition = startPosition - new Vector2(startOffset, 0);
    }

    void Update()
    {
        button.anchoredPosition = Vector2.SmoothDamp(
            button.anchoredPosition,
            targetPosition,
            ref velocity,
            0.15f,
            Mathf.Infinity,
            Time.unscaledDeltaTime
        );
    }
}