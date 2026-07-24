using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;

public class PauseTransition : MonoBehaviour
{
    public RectTransform panel;

    public ButtonTransition[] buttons;

    public bool isPauseOpen = false;

    public float offScreenDistance = 2000f;
    public float overshootAmount = 500f;

    private Vector2 velocity;

    public Vector2 centerPosition;
    public Vector2 targetPosition;

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
            0.15f
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
        // Go slightly past the center
        targetPosition = centerPosition + new Vector2(overshootAmount, 0);

        foreach (ButtonTransition button in buttons)
        {
            button.Enter();
        }

        // Come back after a short delay
        Invoke(nameof(Settle), 0.25f);
    }

    void Settle()
    {
        targetPosition = centerPosition;
    }

    public void Exit()
    {
        isPauseOpen = false;

        targetPosition = centerPosition - new Vector2(offScreenDistance + overshootAmount, 0);

        foreach (ButtonTransition button in buttons)
        {
            button.Exit();
        }
    }
}