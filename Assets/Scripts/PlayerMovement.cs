using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float startSpeed = 10;
    public float sprintSpeed = 15;
    public float speed;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;
    bool isGrounded;
    public float startJumpForce = 3;
    public float startSprintJumpForce = 4;
    private float jumpForce;
    public bool moveLocked;
    public Transform groundCheck;

    void Start()
    {
        speed = startSpeed;
        jumpForce = startJumpForce;
        Cursor.lockState = CursorLockMode.Locked;
    }
    void Update()
    {
        if (!moveLocked)
        {
            isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
            if (Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.W) && isGrounded)
            {
                speed = sprintSpeed;
                jumpForce = startSprintJumpForce;
            }
            else if (isGrounded)
            {
                speed = startSpeed;
                jumpForce = startJumpForce;
            }
            if (Input.GetKey(KeyCode.Q))
            {
                GetComponent<Rigidbody>().AddForce(transform.up * -speed * Time.deltaTime);
            }
            if (Input.GetKey(KeyCode.E))
            {
                GetComponent<Rigidbody>().AddForce(transform.up * speed * Time.deltaTime);
            }

            float x = Input.GetAxis("Horizontal");
            float z = Input.GetAxis("Vertical");

            Vector3 move = transform.right * x + transform.forward * z;

            transform.GetComponent<Rigidbody>().AddForce(move * speed * Time.deltaTime);

            if (Input.GetButtonDown("Jump") && isGrounded)
            {
                transform.GetComponent<Rigidbody>().AddForce(transform.up * jumpForce);
            }
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.C))
            {
                moveLocked = false;
                transform.parent = null;
                GetComponent<CapsuleCollider>().enabled = true;
                gameObject.AddComponent<Rigidbody>();
                GetComponent<Rigidbody>().useGravity = true;
                GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeRotation;
                transform.rotation = Quaternion.identity;
                transform.GetChild(0).rotation = Quaternion.identity;
                GetComponent<ShipController>().toFirstPerson();
                GetComponent<ShipController>().throttle = 0;
            }
        }
    }
}
