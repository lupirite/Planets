using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GravityEffector : MonoBehaviour
{
    public float g;

    private void Update()
    {
        foreach (GravityObject obj in GravityObject.gravityObjects)
        {
            Vector3 dir = transform.position - obj.transform.position;
            obj.GetComponent<Rigidbody>().AddForce(dir.normalized * g / Mathf.Pow(dir.magnitude, 2));
        }
    }
}
