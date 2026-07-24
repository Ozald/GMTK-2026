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

    private bool entering;

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
        CancelInvoke();

        Invoke(nameof(StartEnter), delay);
    }

    void StartEnter()
    {
        entering = true;

        targetPosition = startPosition + new Vector2(overshootAmount, 0);

        Invoke(nameof(Settle), 0.15f);
    }

    void Settle()
    {
        targetPosition = startPosition;
    }

    public void Exit()
    {
        CancelInvoke();

        entering = false;

        // Slide away left
        targetPosition = startPosition - new Vector2(startOffset, 0);
    }

    void Update()
    {
        button.anchoredPosition = Vector2.SmoothDamp(
            button.anchoredPosition,
            targetPosition,
            ref velocity,
            0.15f
        );
    }
}