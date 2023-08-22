using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class navBall : MonoBehaviour
{
    void Update()
    {
        Transform planet = SphereGenerator.getNearest(transform.position);
        Vector3 up = -(planet.position - transform.position).normalized;
        transform.forward = up;
    }
}
