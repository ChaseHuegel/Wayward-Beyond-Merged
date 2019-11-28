using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Billboard : MonoBehaviour
{
	public bool lockAxis = true;

	public bool useFacing = true;
	public Vector3 facing = Vector3.forward;
	public float facingValue = 0.0f;

	public Quaternion direction;

	//	return 1 if facing, -1 if facing opposite Vector3.Dot(transform.forward, (other.position - transform.position).normalized);

    private void LateUpdate()
    {
		direction = Camera.main.transform.rotation;

		if (lockAxis == true)
		{
			// Vector3 v = cameraToLookAt.transform.position - transform.position;
			// float originalZ = transform.rotation.eulerAngles.z;
			// v.x = v.z = 0.0f;
			// transform.LookAt(cameraToLookAt.transform.position - v);
			// transform.Rotate(0, 0, originalZ);
			transform.rotation = direction;
		}
		else
		{
			transform.rotation = direction;
		}

		if (useFacing == true)
		{
			facingValue = Vector3.Dot(facing, (Camera.main.transform.position - transform.position).normalized);

			float yScale = 1.0f;
			float xScale = 1.0f;

			if (facingValue < 0)
			{
				yScale = -1.0f;
			}

			transform.localScale = new Vector3(xScale, transform.localScale.y, yScale);

			Debug.DrawRay(transform.position, transform.rotation.eulerAngles.normalized, Color.blue);
			Debug.DrawRay(transform.position, (Camera.main.transform.position - transform.position).normalized, Color.yellow);
		}
    }
}
