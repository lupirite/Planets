using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class powerSwitch : MonoBehaviour
{
    public Vector3 onPos;

    public Material onMaterial;
    public Material offMaterial;

    [HideInInspector]
    public bool engineOn;

    private Vector3 offPos;

    private List<GameObject> lightObjectsToTurnOff = new List<GameObject>();
    private List<GameObject> lightObjectsToDisable = new List<GameObject>();

    public void Start()
    {
        offPos = transform.localPosition;
        AddDescendantsWithLayer(transform.parent, "TurnOffOnShipShutdown", lightObjectsToTurnOff);
        AddDescendantsWithLayer(transform.parent, "DisableOnShipShutdown", lightObjectsToDisable);
        LightsOn(false);
        LightsEnabled(false);
    }

    public void pressButton()
    {
        if (transform.localPosition == offPos)
        {
            transform.localPosition = onPos;
            engineOn = true;
        }
        else
        {
            transform.localPosition = offPos;
            engineOn = false;
            
        }
        GameObject.Find("Player").GetComponent<ShipController>().engineOn = engineOn;
        LightsOn(engineOn);
        LightsEnabled(engineOn);
    }

    private void AddDescendantsWithLayer(Transform parent, string layer, List<GameObject> list)
    {
        foreach (Transform child in parent)
        {
            if (child.gameObject.layer == LayerMask.NameToLayer(layer))
            {
                list.Add(child.gameObject);
            }
            AddDescendantsWithLayer(child, layer, list);
        }
    }

    private void LightsEnabled(bool active)
    {
        foreach (GameObject light in lightObjectsToDisable)
        {
            light.SetActive(active);
        }
    }

    private void LightsOn(bool active)
    {
        foreach (GameObject light in lightObjectsToTurnOff)
        {
            if (active)
            {
                light.GetComponent<MeshRenderer>().material = onMaterial;
            }
            else
            {
                light.GetComponent<MeshRenderer>().material = offMaterial;
            }
        }
    }
}
