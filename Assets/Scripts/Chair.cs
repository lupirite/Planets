using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chair : MonoBehaviour
{
    public Vector3 offset;

    public Transform sitRot;

    public void pressButton()
    {
        GameObject player = GameObject.Find("Player");
        player.transform.parent = transform;
        player.GetComponent<PlayerMovement>().moveLocked = true;
        player.GetComponent<CapsuleCollider>().enabled = false;
        player.transform.eulerAngles = -transform.parent.right;
        player.transform.GetChild(0).eulerAngles = -transform.parent.right;
        Destroy(player.GetComponent<Rigidbody>());
        player.transform.position = transform.position + transform.rotation*offset;
        player.transform.rotation = sitRot.rotation;
    }
}
