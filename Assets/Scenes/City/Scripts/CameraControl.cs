using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour
{
    public float zoomSpeed = 100;
    public float targetOrtho;
    public float smoothSpeed = 200.0f;
    public float minOrtho = 1.0f;
    public float maxOrtho = 250.0f;

    private bool edge_scrolling = false;

    void Start()
    {
        targetOrtho = Camera.main.orthographicSize;
    }

    void Update()
    {

        if (Input.GetKeyDown(KeyCode.Space))
        {
            edge_scrolling = !edge_scrolling;
        }

        if (edge_scrolling)
        {
            float mouseX = (Input.mousePosition.x / Screen.width) - 0.5f;
            float mouseY = (Input.mousePosition.y / Screen.height) - 0.5f;
            if (Mathf.Abs(mouseX) < 0.25) mouseX = 0;
            if (Mathf.Abs(mouseY) < 0.25) mouseY = 0;
            transform.position = transform.position + 10f * (new Vector3(mouseX, mouseY, 0));
        }

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0.0f)
        {
            targetOrtho -= scroll * zoomSpeed;
            targetOrtho = Mathf.Clamp(targetOrtho, minOrtho, maxOrtho);
        }

        Camera.main.orthographicSize = Mathf.MoveTowards(Camera.main.orthographicSize, targetOrtho, smoothSpeed * Time.deltaTime);

    }
}
