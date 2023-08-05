using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamController : MonoBehaviour
{
    public float velocityEffect;

    public float camDist;

    public float lerpSpeed;

    public float sensitivity = 100;

    public LayerMask hitMask;

    public Transform player;
    public Transform head;

    float x, y;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        x += Input.GetAxis("Mouse X") * Time.deltaTime * sensitivity;
        y += Input.GetAxis("Mouse Y") * Time.deltaTime * sensitivity;

        if (y + Input.GetAxis("Mouse Y") * Time.deltaTime * sensitivity < 90 && y + Input.GetAxis("Mouse Y") * Time.deltaTime * sensitivity > -90)
            y += Input.GetAxis("Mouse Y") * Time.deltaTime * sensitivity;
        x += Input.GetAxis("Mouse X") * Time.deltaTime * sensitivity;

        transform.eulerAngles = new Vector3(-y, x, 0);
        Vector3 targetPos = head.position - transform.forward * (camDist + player.GetComponent<Rigidbody>().velocity.magnitude * velocityEffect);

        RaycastHit hit;
        if (Physics.Raycast(player.position, targetPos - player.position, out hit, (targetPos - player.position).magnitude, hitMask))
        {
            targetPos = hit.point;
        }

        transform.position = targetPos;
    }
}
