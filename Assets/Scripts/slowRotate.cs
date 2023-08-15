using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class slowRotate : MonoBehaviour
{
    public float rotationSpeed = 100;

    void Update()
    {
        transform.localEulerAngles = new Vector3(0, Time.timeSinceLevelLoad*rotationSpeed, 0);
    }
}
