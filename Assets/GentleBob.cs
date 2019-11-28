using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GentleBob : MonoBehaviour
{
    public bool isEnabled = false;
    public float strength = 0.1f;

    private Vector3 baseLocation = Vector3.zero;
    private Vector3 offset = Vector3.zero;

    private void Start()
    {
        baseLocation = this.transform.position;
    }

    private void Update()
    {
        offset.x = Mathf.Sin(Time.time) *strength;
        offset.y = Mathf.Sin(Time.time) * strength;
        offset.x *= Mathf.Cos(Time.time);

        this.transform.position = baseLocation + offset;
    }
}
