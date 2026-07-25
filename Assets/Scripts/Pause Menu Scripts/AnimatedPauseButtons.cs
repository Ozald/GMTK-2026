using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class AnimatedButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Image highlightImage;

    public float slideDistance = 50f;
    public float slideSpeed = 10f;

    private Vector3 targetPosition;
    private Vector3 startPosition;

    private RectTransform highlightRect;

    void Start()
    {
        highlightRect = highlightImage.rectTransform;

        startPosition = highlightRect.position;
        targetPosition = startPosition;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!PauseTransition.isPauseOpen)
            return;
        MoveHighlight();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        targetPosition = startPosition;
    }

    public void ResetHighlight()
    {
        targetPosition = startPosition;
    }

    void MoveHighlight()
    {
        RectTransform buttonRect = GetComponent<RectTransform>();

        // Copy the button's rotation
        highlightRect.rotation = buttonRect.rotation;

        // Get the button's Z rotation
        float angle = buttonRect.eulerAngles.z * Mathf.Deg2Rad;

        // Create a direction based on that angle
        Vector3 direction = new Vector3(
            Mathf.Cos(angle),
            Mathf.Sin(angle),
            0
        );

        // Move along the rotated axis
        targetPosition = buttonRect.position + direction * -10f;
    }

    void Update()
    {
        if (highlightRect != null)
        {
            highlightRect.position = Vector3.Lerp(
                highlightRect.position,
                targetPosition,
                Time.unscaledDeltaTime * slideSpeed
            );
        }
    }
}