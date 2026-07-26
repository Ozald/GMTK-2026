using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class AnimatedTitleButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Image highlightImage;

    public float slideSpeed = 10f;

    private Vector3 targetPosition;
    private Vector3 startPosition;

    private RectTransform highlightRect;


    void Start()
    {
        if (highlightImage == null)
        {
            Debug.LogWarning("No highlight image assigned on " + gameObject.name);
            return;
        }

        highlightRect = highlightImage.rectTransform;

        startPosition = highlightRect.position;
        targetPosition = startPosition;
    }


    public void OnPointerEnter(PointerEventData eventData)
    {
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


    private void MoveHighlight()
    {
        RectTransform buttonRect = GetComponent<RectTransform>();

        highlightRect.rotation = buttonRect.rotation;

        float angle = buttonRect.eulerAngles.z * Mathf.Deg2Rad;

        Vector3 direction = new Vector3(
            Mathf.Cos(angle),
            Mathf.Sin(angle),
            0
        );

        targetPosition = buttonRect.position + direction * -10f;
    }


    void Update()
    {
        if (highlightRect == null)
            return;

        highlightRect.position = Vector3.Lerp(
            highlightRect.position,
            targetPosition,
            Time.unscaledDeltaTime * slideSpeed
        );
    }
}