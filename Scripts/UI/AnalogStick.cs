using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Custom UI element for replicating a physical controller analog stick.
/// Must be attached to the inner analog element.
/// </summary>
public class AnalogStick : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    private bool isDragging = false;
    private float analogAreaRadius = 0.0f;
    private Image analogArea, analogKnob;
    private Vector2 analogInputValues = Vector2.zero;

    public OnValueChanged onValueChanged;

    void Start()
    {
        analogKnob = transform.GetChild(0).GetComponent<Image>();
        analogArea = GetComponent<Image>();
        analogAreaRadius = (analogArea.rectTransform.rect.width / 2.0f * FindObjectOfType<Canvas>().scaleFactor) -
                           (analogKnob.rectTransform.rect.width / 2.0f * FindObjectOfType<Canvas>().scaleFactor);
    }

    void Update()
    {
        if (isDragging)
        {
            analogKnob.transform.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
            ClampAimKnob();
        }
    }

    /// <summary>
    /// Checks if the knob is within the radius of the measurement area. If not, corrects the position.
    /// Then clamps the analog values between -1 and 1 and assigns them to a publicly accessible variable.
    /// </summary>
    private void ClampAimKnob()
    {
        float distFromCenter = Vector2.Distance(analogArea.transform.position, analogKnob.transform.position);
        if (distFromCenter > analogAreaRadius)
        {
            Vector2 directionToCenter = analogKnob.transform.position - analogArea.transform.position;
            directionToCenter *= analogAreaRadius / distFromCenter;
            analogKnob.transform.position = analogArea.transform.position + (Vector3)directionToCenter;
        }
        analogInputValues.x = Mathf.Clamp((analogKnob.transform.position - analogArea.transform.position).x / analogAreaRadius, -1.0f, 1.0f);
        analogInputValues.y = Mathf.Clamp((analogKnob.transform.position - analogArea.transform.position).y / analogAreaRadius, -1.0f, 1.0f);

        onValueChanged.Invoke(analogInputValues);
    }

    /// <summary>
    /// Initiates dragging when a click is detected inside the measurement area.
    /// </summary>
    /// <param name="eventData">Pointer Event data.</param>
    public void OnPointerDown(PointerEventData eventData)
    {
        isDragging = true;
    }

    /// <summary>
    /// Termiantes dragging when the selecting pointer is released.
    /// </summary>
    /// <param name="eventData">Pointer Event data.</param>
    public void OnPointerUp(PointerEventData eventData)
    {
        isDragging = false;
        analogKnob.transform.localPosition = Vector3.zero;
    }
}

/// <summary>
/// Proxy class for calling event listeners.
/// </summary>
[Serializable]
public class OnValueChanged : UnityEvent<Vector2> { };
