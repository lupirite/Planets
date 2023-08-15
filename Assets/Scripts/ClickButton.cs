using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClickButton : MonoBehaviour
{
    public LayerMask clickMask;
    public LayerMask ignoreMask;
    public float maxButtonDistance = 3;
    public GameObject opclObject;
    public GameObject stchrObject;
    public GameObject engnonObject;

    private void Update()
    {
        opclObject.SetActive(false);
        stchrObject.SetActive(false);
        engnonObject.SetActive(false);
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, maxButtonDistance, ignoreMask))
        {
            if ((clickMask.value & (1 << hit.transform.gameObject.layer)) > 0)
            {
                if (hit.transform.GetComponent<AnimTrigger>() != null)
                {
                    opclObject.SetActive(true);
                }
                if (hit.transform.GetComponent<Chair>() != null && hit.transform.childCount == 0)
                {
                    stchrObject.SetActive(true);
                }
                if (hit.transform.GetComponent<powerSwitch>() != null && hit.transform.childCount == 0)
                {
                    engnonObject.SetActive(true);
                }

                if (Input.GetKeyDown(KeyCode.F))
                {
                    if (hit.transform.GetComponent<AnimTrigger>() != null)
                    {
                        hit.transform.GetComponent<AnimTrigger>().pressButton();
                    }
                    if (hit.transform.GetComponent<Chair>() != null)
                    {
                        hit.transform.GetComponent<Chair>().pressButton();
                    }
                    if (hit.transform.GetComponent<powerSwitch>() != null)
                    {
                        hit.transform.GetComponent<powerSwitch>().pressButton();
                    }
                }
            }
        }
    }
}
