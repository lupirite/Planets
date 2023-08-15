using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GravityEffector : MonoBehaviour
{
    public float g;
    public GameObject player;

    private void Update()
    {
        foreach (GravityObject obj in GravityObject.gravityObjects)
        {
            Vector3 dir = transform.position - player.transform.position;

            obj.GetComponent<Rigidbody>().AddForce(-player.transform.up * g * 10000 * obj.GetComponent<Rigidbody>().mass / Mathf.Pow(dir.magnitude, 2) / 4);
        }
    }
}
