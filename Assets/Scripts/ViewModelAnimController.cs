using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ViewModelAnimController : MonoBehaviour
{
	public Animator viewModelAnimator;

    void Update()
    {
        if (Input.GetMouseButton(0) == true)
		{
			viewModelAnimator.Play("swing");
		}

		if (Input.GetKeyDown(KeyCode.Escape) == true)
		{
			Application.Quit();
		}
    }
}
