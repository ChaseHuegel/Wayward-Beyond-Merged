using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseInspect : MonoBehaviour
{
    public float speed = 1.0f;

    public void Update()
    {
        if (Input.GetMouseButton(0) == true)
        {
            transform.RotateAround(transform.position, Camera.main.transform.up, Input.GetAxis("Mouse X") * Time.deltaTime * -speed);
            transform.RotateAround(transform.position, Camera.main.transform.right, Input.GetAxis("Mouse Y") * Time.deltaTime * speed);
        }
    }
}
