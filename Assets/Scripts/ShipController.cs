using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipController : MonoBehaviour
{
    public float throttleSensitivity = 10;
    public float torque = 100;
    public float engineThrust;
    public float maxThrottle;

    public float throttle;
    private GameObject chair;
    private bool firstPerson = true;

    [HideInInspector]
    public bool engineOn;

    void Update()
    {
        if (transform.GetComponentInParent<Chair>() != null)
        {
            if (engineOn)
            {
                chair = transform.parent.gameObject;
                GameObject ship = chair.transform.parent.gameObject;
                if (Input.GetKey(KeyCode.LeftShift))
                {
                    throttle += throttleSensitivity * Time.deltaTime;
                    if (throttle > maxThrottle)
                        throttle = maxThrottle;
                }
                if (Input.GetKey(KeyCode.LeftControl))
                {
                    throttle -= throttleSensitivity * Time.deltaTime;
                    if (throttle < 0)
                        throttle = 0;
                }
                if (Input.GetKey(KeyCode.X))
                {
                    throttle = 0;
                }
                if (Input.GetKey(KeyCode.Z))
                {
                    throttle = maxThrottle;
                }

                if (Input.GetKey(KeyCode.W))
                {
                    transform.parent.parent.GetComponent<Rigidbody>().AddRelativeTorque(0, 0, torque * Time.deltaTime);
                }
                if (Input.GetKey(KeyCode.S))
                {
                    transform.parent.parent.GetComponent<Rigidbody>().AddRelativeTorque(0, 0, -torque * Time.deltaTime);
                }
                if (Input.GetKey(KeyCode.A))
                {
                    transform.parent.parent.GetComponent<Rigidbody>().AddRelativeTorque(0, -torque * Time.deltaTime, 0);
                }
                if (Input.GetKey(KeyCode.D))
                {
                    transform.parent.parent.GetComponent<Rigidbody>().AddRelativeTorque(0, torque * Time.deltaTime, 0);
                }
                if (Input.GetKey(KeyCode.Q))
                {
                    transform.parent.parent.GetComponent<Rigidbody>().AddRelativeTorque(-torque * Time.deltaTime, 0, 0);
                }
                if (Input.GetKey(KeyCode.E))
                {
                    transform.parent.parent.GetComponent<Rigidbody>().AddRelativeTorque(torque * Time.deltaTime, 0, 0);
                }

                ship.GetComponent<Rigidbody>().AddForce(-ship.transform.right * throttle * engineThrust * Time.deltaTime);
            }
            else
            {
                throttle = 0;
            }

            if (Input.GetKeyDown(KeyCode.V) && transform.parent != null)
            {
                if (firstPerson)
                {
                    toThirdPerson();
                }
                else
                {
                    toFirstPerson();
                }

                firstPerson = !firstPerson;
            }
        }
        else
        {
            chair = null;
        }
    }

    public void toFirstPerson()
    {
        chair.transform.parent.Find("CameraHolder").gameObject.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void toThirdPerson()
    {
        chair.transform.parent.Find("CameraHolder").gameObject.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
    }
}
