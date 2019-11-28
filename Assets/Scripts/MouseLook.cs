using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseLook : MonoBehaviour
{
	public Transform head;
	public Transform body;

    public float cameraSensitivity = 90;

	public float rotationX = 0.0f;
	public float rotationY = 0.0f;

	void Start ()
	{
		Cursor.lockState = CursorLockMode.Locked;
	}
 
	void Update ()
	{
		if (Cursor.lockState == CursorLockMode.Locked)
		{
			rotationX += Input.GetAxis("Mouse X") * cameraSensitivity * Time.deltaTime;
			rotationY += Input.GetAxis("Mouse Y") * cameraSensitivity * Time.deltaTime;
			rotationY = Mathf.Clamp (rotationY, -65, 90);
		}
 
		body.localRotation = Quaternion.AngleAxis(rotationX, Vector3.up);
		head.localRotation = Quaternion.AngleAxis(rotationY, Vector3.left);
 
		if (Input.GetKeyDown (KeyCode.T))
		{
			Cursor.lockState = (Cursor.lockState == CursorLockMode.Locked) ? CursorLockMode.None : CursorLockMode.Locked;
		}
	}
}
