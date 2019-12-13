using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CameraController : MonoBehaviour
{
    private bool cameraUsable = true;
    private Bounds bgBounds;
    private float[] edgeBounds = new float[4]; // L, R, T, B
    private Vector3 mouseStartPosition = Vector3.zero;

    public float dragSpeed = 1.0f;

    // Start is called before the first frame update
    void Start()
    {
        CalculateCameraBounds();
    }

    // Update is called once per frame
    void Update()
    {
        if (!EventSystem.current.IsPointerOverGameObject() && cameraUsable)
        {
            if (Input.mouseScrollDelta.y > 0 && Camera.main.orthographicSize > 15)
            {
                Camera.main.orthographicSize--;
                CalculateCameraBounds();
            }
            else if (Input.mouseScrollDelta.y < 0 && Camera.main.orthographicSize < 35)
            {
                Camera.main.orthographicSize++;
                CalculateCameraBounds();
            }

            if (Input.GetMouseButtonDown(0))
            {
                mouseStartPosition = Input.mousePosition;
                return;
            }

            if (!Input.GetMouseButton(0)) { return; }

            Vector3 position = Camera.main.ScreenToViewportPoint(Input.mousePosition - mouseStartPosition);
            Vector3 move = new Vector3(position.x * dragSpeed, position.y * dragSpeed, 0);
            transform.Translate(-move, Space.World);
        }
        else
        {
            if (Input.GetMouseButton(0)) { cameraUsable = false; }
        }

        if(Input.GetMouseButtonUp(0) && !cameraUsable) { cameraUsable = true; }
    }

    private void LateUpdate()
    {
        if (!EventSystem.current.IsPointerOverGameObject())
        {
            Vector3 currentPosition = transform.position;
            currentPosition.x = Mathf.Clamp(currentPosition.x, edgeBounds[0], edgeBounds[1]);
            currentPosition.y = Mathf.Clamp(currentPosition.y, edgeBounds[3], edgeBounds[2]);
            transform.position = currentPosition;
            mouseStartPosition = Input.mousePosition;
        }
    }

    private void CalculateCameraBounds()
    {
        float height = Camera.main.orthographicSize;
        float width = height * Screen.width / Screen.height;
        bgBounds = GameObject.FindWithTag("Background").GetComponent<SpriteRenderer>().bounds;
        edgeBounds[0] = width - bgBounds.size.x / 2.0f;
        edgeBounds[1] = bgBounds.size.x / 2.0f - width;
        edgeBounds[2] = bgBounds.size.y / 2.0f - height;
        edgeBounds[3] = height - bgBounds.size.y / 2.0f;
    }
}
