using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipCam : MonoBehaviour
{
    public float sensitivity = 100f;
    public float scrollSensitivity = 1;
    public float minDistToShip = 3;

    float xRotation = 0f;
    float yRotation = 0f;
    public bool inverted = true;
    private int invert = 1;
    public GameObject player;

    private Vector3 shipPrevRot;
    
    // Start is called before the first frame update
    void Start()
    {
        if (inverted)
        {
            invert = -1;
        }
    }

    // Update is called once per frame
    void Update()
    {
        transform.parent.rotation = Quaternion.identity;
        transform.rotation = player.transform.rotation;
        transform.LookAt(player.transform.position, gameObject.transform.up);
        if (Input.GetMouseButton(1))
        {
            float mouseX = Input.GetAxis("Mouse X") * sensitivity * Time.deltaTime / Time.timeScale;
            float mouseY = Input.GetAxis("Mouse Y") * sensitivity * Time.deltaTime / Time.timeScale;

            xRotation = mouseY * sensitivity;
            yRotation = mouseX * invert * sensitivity;

            transform.RotateAround(player.transform.position, gameObject.transform.up, yRotation * Time.deltaTime / Time.timeScale);

            transform.RotateAround(player.transform.position, transform.right, xRotation * Time.deltaTime / Time.timeScale);
        }
        if (Input.mouseScrollDelta.y > 0)
        {
            if (Vector3.Distance(gameObject.transform.position, transform.parent.position) > minDistToShip)
                gameObject.transform.position = Vector3.MoveTowards(gameObject.transform.position, transform.parent.position, Vector3.Distance(gameObject.transform.position, transform.parent.position) * scrollSensitivity / 10 + 1 * Time.deltaTime / Time.timeScale);
        }
        else if (Input.mouseScrollDelta.y < 0)
        {
            gameObject.transform.position = Vector3.MoveTowards(gameObject.transform.position, transform.parent.position, -Vector3.Distance(gameObject.transform.position, transform.parent.position) * scrollSensitivity / 10 + 1 * Time.deltaTime / Time.timeScale);
        }

        shipPrevRot = transform.parent.parent.eulerAngles;
    }
}
