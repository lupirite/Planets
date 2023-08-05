using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class webSpinner : MonoBehaviour
{
    public LayerMask hitMask;

    public float maxDist;

    public float ropeStiffness;
    public float ropeRetraction;

    public float throwForce;

    public GameObject webEnd;

    public LineRenderer visual;

    private Vector3 stickPos;
    public float ropeLength;
    private bool rope;

    public bool isRight = true;

    private Rigidbody rb;
    private Transform end;
    private Vector3 lastPos;
    private Rigidbody otherRB;
    private List<Vector3> wrapPositions = new List<Vector3>();
    private List<Vector3> normals = new List<Vector3>();

    private bool wasPressed;
    private void Start()
    {
        rb = GetComponentInParent<Rigidbody>();
        visual.enabled = false;

        wrapPositions.Add(Vector3.zero);
        normals.Add(Vector3.zero);
    }
    private void Update()
    {
        //visual.SetPosition(0, transform.position);

        float distToStick = 0;
        for (int i = 1; i < wrapPositions.Count; i++)
        {
            distToStick += (wrapPositions[i] - wrapPositions[i - 1]).magnitude;
        }

        if ((Input.GetAxis("XR_Right_Trigger_Axis")>.8&&isRight)||(Input.GetAxis("XR_Left_Trigger_Axis")>.8&&!isRight))
        {
            if (!wasPressed)
            {
                wasPressed = true;
                rope = false;
                visual.enabled = false;
                wrapPositions = new List<Vector3>();
                wrapPositions.Add(Vector3.zero);
                normals = new List<Vector3>();
                normals.Add(Vector3.zero);
                if (end)
                    Destroy(end.gameObject);

                end = Instantiate(webEnd, transform.position, Quaternion.identity, null).transform;
                end.GetComponent<Rigidbody>().velocity = transform.forward * throwForce + rb.velocity;
                rb.AddForce(-transform.forward * throwForce * .01f);

                visual.enabled = true;
                lastPos = transform.position;
            }
        }
        else
        {
            wasPressed = false;
        }

        try
        {
            var leftHandDevices = new List<UnityEngine.XR.InputDevice>();
            UnityEngine.XR.InputDevices.GetDevicesAtXRNode(UnityEngine.XR.XRNode.LeftHand, leftHandDevices);
            var rightHandDevices = new List<UnityEngine.XR.InputDevice>();
            UnityEngine.XR.InputDevices.GetDevicesAtXRNode(UnityEngine.XR.XRNode.RightHand, rightHandDevices);

            bool grip;
            UnityEngine.XR.InputDevice device;
            device = leftHandDevices[0];
            if (rightHandDevices.Count == 1 && isRight)
            {
                device = rightHandDevices[0];
            }

            if (device.TryGetFeatureValue(UnityEngine.XR.CommonUsages.gripButton, out grip) && grip)
            {
                rope = false;
                visual.enabled = false;
                wrapPositions = new List<Vector3>();
                wrapPositions.Add(Vector3.zero);
                normals = new List<Vector3>();
                normals.Add(Vector3.zero);
                if (end)
                    Destroy(end.gameObject);
                end = null;
            }
        }
        catch
        {

        }

        if (rope)
        {
            if (((Input.GetAxis("XR_Right_Trigger_Axis")>.8 && isRight) || (Input.GetAxis("XR_Left_Trigger_Axis")>.8 && !isRight)) && ropeLength > 2)
            {
                ropeLength -= ropeRetraction * Time.deltaTime;
            }

            stickPos = end.position - end.forward / 2;
            wrapPositions[0] = stickPos;

            distToStick = (transform.position-wrapPositions[wrapPositions.Count-1]).magnitude;
            for (int i = 1; i < wrapPositions.Count; i++)
            {
                distToStick += (wrapPositions[i] - wrapPositions[i - 1]).magnitude;
            }

            //visual.SetPosition(1, stickPos);

            Vector3 force = (wrapPositions[wrapPositions.Count-1] - transform.position).normalized * Mathf.Max(0,  distToStick - ropeLength) * Time.deltaTime * ropeStiffness / (ropeLength + 1);
            rb.AddForceAtPosition(force, transform.position);

            Vector3 pos = transform.position;
            if (wrapPositions.Count > 1)
            {
                pos = wrapPositions[1];
            }
            force = (pos - end.position).normalized * Mathf.Max(0, distToStick - ropeLength) * Time.deltaTime * ropeStiffness / (ropeLength + 1);
            if (otherRB)
            {
                otherRB.AddForceAtPosition(force, wrapPositions[0]);
            }

            RaycastHit hit;
            if (Physics.Raycast(transform.position, wrapPositions[wrapPositions.Count-1]-transform.position, out hit, (wrapPositions[wrapPositions.Count - 1] - transform.position).magnitude, hitMask) && !hit.transform.GetComponent<Rigidbody>())
            {
                if ((hit.point - wrapPositions[wrapPositions.Count - 1]).magnitude > 0 && hit.normal != normals[normals.Count - 1])
                {
                    wrapPositions.Add(hit.point);
                    normals.Add(hit.normal);
                }
            }

            if (wrapPositions.Count > 1 && !Physics.Raycast(transform.position, wrapPositions[wrapPositions.Count - 2] - transform.position, out hit, (wrapPositions[wrapPositions.Count - 2] - transform.position).magnitude, hitMask))
            {
                wrapPositions.RemoveAt(wrapPositions.Count-1);
                normals.RemoveAt(wrapPositions.Count - 1);
            }
        }
        else
        {
            if (end)
            {
                wrapPositions[0] = end.position;

                RaycastHit hit;
                if (Physics.Raycast(lastPos, end.position - lastPos, out hit, (end.position - lastPos).magnitude, hitMask))
                {
                    wrapPositions = new List<Vector3>();
                    rope = true;
                    stickPos = hit.point;
                    wrapPositions.Add(stickPos);
                    normals.Add(hit.normal);

                    end.parent = hit.transform;
                    Destroy(end.GetComponent<Rigidbody>());
                    end.position = hit.point + end.forward / 2;

                    if (hit.transform.GetComponent<Rigidbody>())
                    {
                        otherRB = hit.transform.GetComponent<Rigidbody>();
                    }
                    else
                    {
                        otherRB = null;
                    }

                    distToStick = (transform.position - stickPos).magnitude;

                    visual.SetPosition(1, stickPos);
                    ropeLength = distToStick;
                }
                lastPos = end.position;
            }
        }

        Vector3[] lst = new Vector3[wrapPositions.Count + 1];
        for (int i = 0; i < wrapPositions.Count; i++)
        {
            lst[i] = wrapPositions[i];
        }
        lst[wrapPositions.Count] = transform.position;
        visual.positionCount = lst.Length;
        visual.SetPositions(lst);
    }
}
