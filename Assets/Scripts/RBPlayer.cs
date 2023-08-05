using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class RBPlayer : MonoBehaviour
{
    public GameObject planet;

    public LayerMask groundMask;
    public float groundDist = .1f;
    public Transform groundCheck;
    public Transform cam;

    public float standForce;

    public float jumpForce;
    public AnimationCurve walkForce;
    public AnimationCurve sprintForce;

    public float rotateSpeed = 100;

    private float jumpTimer;

    Rigidbody rb;
    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    public Vector3 getUpVector()
    {
        return (ScaleManager.instance.celestialCamera.transform.position - planet.transform.position).normalized;
    }
    
    private void Update()
    {
        Vector3 upVector = getUpVector();
        Debug.DrawRay(transform.position, upVector);
        // Rotate up
        transform.rotation = Quaternion.FromToRotation(transform.up, upVector);
        //Vector3 axis = Vector3.Cross(transform.up, upVector);
        //rb.AddTorque(axis*Time.deltaTime*standForce);

        transform.RotateAroundLocal(transform.up, Input.GetAxis("XR_Right_Primary2DAxisX")*Time.deltaTime*rotateSpeed);

        // Check if grounded
        bool grounded = Physics.CheckSphere(groundCheck.position, groundDist, groundMask);

        // Movement
        if (grounded)
        {
            if ((Input.GetKey(KeyCode.Space)||Input.GetButton("Submit")) && jumpTimer >= .25)
            {
                rb.AddForce(transform.up * jumpForce);
                jumpTimer = 0;
            }
            float h = Input.GetAxis("XR_Left_Primary2DAxisX");
            float v = Input.GetAxis("XR_Left_Primary2DAxisY");

            float moveForce = walkForce.Evaluate(rb.velocity.magnitude);

            if (Input.GetKey(KeyCode.LeftShift) || Input.GetButton("XR_Left_Primary2DAxisClick"))
            {
                moveForce = sprintForce.Evaluate(rb.velocity.magnitude);
            }

            rb.AddForce(transform.rotation * new Vector3(h, 0, v) * Time.deltaTime * moveForce);
        }

        if (jumpTimer < .25)
            jumpTimer += Time.deltaTime;
    }
}
