using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class VRMap
{
    public Transform vrTarget;
    public Transform rigTarget;
    public Vector3 trackingPositionOffset;
    public Vector3 trackingRotationOffset;

    public void Map()
    {
        rigTarget.position = vrTarget.TransformPoint(trackingPositionOffset);
        rigTarget.rotation = vrTarget.rotation * Quaternion.Euler(trackingRotationOffset);
    }
}

public class VRRig : MonoBehaviour
{
    public VRMap head;
    public VRMap leftHand;
    public VRMap rightHand;

    public Transform headConstraint;
    public Vector3 headBodyOffset;

    public float turnSmoothness;
    
    public float limitAngle = 20;
    public float minAngle = 5;

    private bool turning;

    private void Start()
    {
        headBodyOffset = transform.position - headConstraint.position;
    }

    private void Update()
    {
        transform.position = headConstraint.position + headBodyOffset;

        if (Vector3.Angle(transform.forward, Vector3.ProjectOnPlane(headConstraint.up, Vector3.up).normalized) > limitAngle)
        {
            turning = true;
        }
        else if (Vector3.Angle(transform.forward, Vector3.ProjectOnPlane(headConstraint.up, Vector3.up).normalized) < minAngle)
        {
            turning = false;
        }

        if (turning)
            transform.forward = Vector3.Lerp(transform.forward, Vector3.ProjectOnPlane(headConstraint.up, Vector3.up).normalized, Time.deltaTime * turnSmoothness);

        head.Map();
        leftHand.Map();
        rightHand.Map();
    }
}
