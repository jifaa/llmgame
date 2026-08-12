using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonHoverEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [Header("Hover Settings")]
    public Vector3 hoverScale = new Vector3(1.05f, 1.05f, 1f);
    public Vector3 clickScale = new Vector3(0.95f, 0.95f, 1f);
    public float smoothSpeed = 12f;

    private Vector3 targetScale = Vector3.one;
    private Vector3 originalScale = Vector3.one;

    void Start()
    {
        originalScale = transform.localScale;
        targetScale = originalScale;
    }

    void Update()
    {
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * smoothSpeed);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        targetScale = Vector3.Scale(originalScale, hoverScale);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        targetScale = originalScale;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        targetScale = Vector3.Scale(originalScale, clickScale);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        targetScale = Vector3.Scale(originalScale, hoverScale);
    }
}
