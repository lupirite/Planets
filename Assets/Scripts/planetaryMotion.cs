using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class planetaryMotion : MonoBehaviour
{
    public Vector3 angularVelocity;

    private void Update()
    {
        transform.eulerAngles += angularVelocity * Time.deltaTime;
    }
}
