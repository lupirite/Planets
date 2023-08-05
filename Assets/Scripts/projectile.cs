using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class projectile : MonoBehaviour
{
    public bool pointDir;

    private void Update()
    {
        if (pointDir && GetComponent<Rigidbody>())
            transform.forward = -transform.GetComponent<Rigidbody>().velocity;
    }
}
