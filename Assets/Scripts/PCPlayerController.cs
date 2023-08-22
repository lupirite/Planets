using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PCPlayerController : MonoBehaviour
{
    public GameObject planet;
    public float speed;
    public Transform groundCheck;
    public float groundCheckRadius;
    public LayerMask groundMask;
    public AnimationCurve forceWithSpeed;

    public float jumpForce;

    private void Update()
    {
        Vector3 dir = ScaleManager.instance.celestialCamera.transform.position - planet.transform.position;
        Vector3 upVector = dir.normalized;
        transform.up = upVector;

        GetComponent<Rigidbody>().AddForce(-transform.up * planet.GetComponent<GravityEffector>().g*10000 / Mathf.Pow(dir.magnitude, 2)/4);

        // Read WASD inputs
        float moveHorizontal = 0;
        float moveVertical = 0;
        if (Input.GetKey(KeyCode.W))
            moveVertical += 1;
        if (Input.GetKey(KeyCode.S))
            moveVertical -= 1;
        if (Input.GetKey(KeyCode.D))
            moveHorizontal += 1;
        if (Input.GetKey(KeyCode.A))
            moveHorizontal -= 1;

        if (Input.GetKeyDown(KeyCode.Space) && Physics.CheckSphere(groundCheck.position, groundCheckRadius, groundMask)) GetComponent<Rigidbody>().AddForce(upVector * jumpForce);

        GetComponent<Rigidbody>().AddForce((transform.GetChild(0).right*moveHorizontal*Time.deltaTime*speed + transform.GetChild(0).forward*moveVertical*Time.deltaTime*speed)*forceWithSpeed.Evaluate(GetComponent<Rigidbody>().velocity.magnitude));

    }
}
