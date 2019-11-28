using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwingAnim : MonoBehaviour
{
	public Transform target;

	public float speed = 1.0f;
	public float duration = 1.0f;

	public Vector3 rotationStart = new Vector3(0.0f, 0.0f, 0.0f);
	public Vector3 rotationModifier = new Vector3(0.0f, 0.0f, 0.0f);

	public Vector3 positionStart = new Vector3(0.0f, 0.0f, 0.0f);
	public Vector3 positionModifier = new Vector3(0.0f, 0.0f, 0.0f);

	private Vector3 rotation = new Vector3(0.0f, 0.0f, 0.0f);
	private Vector3 position = new Vector3(0.0f, 0.0f, 0.0f);
	private float modifier = 3.5f;
	private bool play = false;
	private bool running = false;

    void Start()
    {
        //baseRotation = transform.rotation.eulerAngles;
    }

    void Update()
    {
        if (Input.GetMouseButton(0) == true)
		{
			play = true;
		}

		if (play == true)
		{
			play = false;
			running = true;
			rotation = new Vector3(0.0f, 0.0f, 0.0f);
		}

		if (running == true && target != null)
		{
			rotation.x += rotationModifier.x * (Time.deltaTime / (duration / 2)) * modifier * speed;
			rotation.y += rotationModifier.y * (Time.deltaTime / (duration / 2)) * modifier * speed;
			rotation.z += rotationModifier.z * (Time.deltaTime / (duration / 2)) * modifier * speed;

			position.x += positionModifier.x * (Time.deltaTime / (duration / 2)) * modifier * speed;
			position.y += positionModifier.y * (Time.deltaTime / (duration / 2)) * modifier * speed;
			position.z += positionModifier.z * (Time.deltaTime / (duration / 2)) * modifier * speed;

			int finishedAxes = 0;

			if (modifier > 0)
			{
				if (rotationModifier.x > 0 && rotation.x >= rotationModifier.x) { finishedAxes++; rotation.x = rotationModifier.x; }
				else if (rotationModifier.x < 0 && rotation.x <= rotationModifier.x) { finishedAxes++; rotation.x = rotationModifier.x; }

				if (rotationModifier.y > 0 && rotation.y >= rotationModifier.y) { finishedAxes++; rotation.y = rotationModifier.y; }
				else if (rotationModifier.y < 0 && rotation.y <= rotationModifier.y) { finishedAxes++; rotation.y = rotationModifier.y; }

				if (rotationModifier.z > 0 && rotation.z >= rotationModifier.z) { finishedAxes++; rotation.z = rotationModifier.z; }
				else if (rotationModifier.z < 0 && rotation.z <= rotationModifier.z) { finishedAxes++; rotation.z = rotationModifier.z; }



				if (positionModifier.x > 0 && position.x >= positionModifier.x) { finishedAxes++; position.x = positionModifier.x; }
				else if (positionModifier.x < 0 && position.x <= positionModifier.x) { finishedAxes++; position.x = positionModifier.x; }

				if (positionModifier.y > 0 && position.y >= positionModifier.y) { finishedAxes++; position.y = positionModifier.y; }
				else if (positionModifier.y < 0 && position.y <= positionModifier.y) { finishedAxes++; position.y = positionModifier.y; }

				if (positionModifier.z > 0 && position.z >= positionModifier.z) { finishedAxes++; position.z = positionModifier.z; }
				else if (positionModifier.z < 0 && position.z <= positionModifier.z) { finishedAxes++; position.z = positionModifier.z; }
			}
			else
			{
				if (rotationModifier.x > 0 && rotation.x <= 0) { finishedAxes++; rotation.x = 0; }
				else if (rotationModifier.x < 0 && rotation.x >= 0) { finishedAxes++; rotation.x = 0; }

				if (rotationModifier.y > 0 && rotation.y <= 0) { finishedAxes++; rotation.y = 0; }
				else if (rotationModifier.y < 0 && rotation.y >= 0) { finishedAxes++; rotation.y = 0; }

				if (rotationModifier.z > 0 && rotation.z <= 0) { finishedAxes++; rotation.z = 0; }
				else if (rotationModifier.z < 0 && rotation.z >= 0) { finishedAxes++; rotation.z = 0; }



				if (positionModifier.x > 0 && position.x <= 0) { finishedAxes++; position.x = 0; }
				else if (positionModifier.x < 0 && position.x >= 0) { finishedAxes++; position.x = 0; }

				if (positionModifier.y > 0 && position.y <= 0) { finishedAxes++; position.y = 0; }
				else if (positionModifier.y < 0 && position.y >= 0) { finishedAxes++; position.y = 0; }

				if (positionModifier.z > 0 && position.z <= 0) { finishedAxes++; position.z = 0; }
				else if (positionModifier.z < 0 && position.z >= 0) { finishedAxes++; position.z = 0; }
			}

			target.localRotation = Quaternion.Euler(rotation + rotationStart);
			target.localPosition = position + positionStart;

			if (finishedAxes == 6)
			{
				if (modifier < 0) { modifier = 3.5f; } else { modifier = -1.0f; }

				if (rotation.x == 0 && rotation.y == 0 && rotation.z == 0 && position.x == 0 && position.y == 0 && position.z == 0)
				{
					running = false;
				}
			}
		}
    }
}
