using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class navBall : MonoBehaviour
{
    void Update()
    {
        transform.rotation = Quaternion.identity * Quaternion.Euler(-90, 0, 0);
    }
}
