using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour
{
    private float zoomSpeed =30f;
    private float targetOrtho = 200f;
    private float smoothSpeed = 2000.0f;
    private float minOrtho = 1f;
    private float maxOrtho = 250.0f;
    private float defaultMoveSpeed = 15f;

    void Start()
    {
        targetOrtho = Camera.main.orthographicSize;
    }

    void Update()
    {
        float moveX = 0;
        float moveY = 0;
        float moveZ = 0;
        float moveSpeed = targetOrtho/maxOrtho * defaultMoveSpeed;

        if (Input.GetKey(KeyCode.W)){
            moveY += moveSpeed;
        }

        if (Input.GetKey(KeyCode.S)){
            moveY -= moveSpeed;
        }

        if (Input.GetKey(KeyCode.A)){
            moveX -= moveSpeed;
        }

        if (Input.GetKey(KeyCode.D)){
            moveX += moveSpeed;
        }

        if (Input.GetKey(KeyCode.Q)){
            moveZ += zoomSpeed;
        }

        if (Input.GetKey(KeyCode.E)){
            moveZ -= zoomSpeed;
        }

        transform.position = Vector3.MoveTowards(transform.position, transform.position + defaultMoveSpeed * (new Vector3(moveX, moveY, 0)), 2*defaultMoveSpeed*Time.deltaTime);

        targetOrtho -= moveZ;
        targetOrtho = Mathf.Clamp(targetOrtho, minOrtho, maxOrtho);

        Camera.main.orthographicSize = Mathf.MoveTowards(Camera.main.orthographicSize, targetOrtho, 2*Mathf.Abs(moveZ*Time.deltaTime));

    }
}
