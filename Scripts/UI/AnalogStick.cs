using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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

    public Vector2 analogInputValues = Vector2.zero;

    void Start()
    {
        analogKnob = transform.GetComponent<Image>();
        analogArea = transform.parent.GetComponent<Image>();
        analogAreaRadius = (analogArea.rectTransform.rect.width / 2.0f * FindObjectOfType<Canvas>().scaleFactor) - 
                           (analogKnob.rectTransform.rect.width / 2.0f * FindObjectOfType<Canvas>().scaleFactor);
    }

    void Update()
    {
        if (isDragging)
        {
            transform.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
            ClampAimKnob();
        }
    }

    /// <summary>
    /// Check if the knob is within the radius of the measurement area. If not, correct the position.
    /// Then, clamp the analog values between -1 and 1 and assign them to a publicly accessible variable.
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
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isDragging = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isDragging = false;
        analogKnob.transform.localPosition = Vector3.zero;
    }
}
