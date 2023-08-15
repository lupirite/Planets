using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class healthBar : MonoBehaviour
{
    public GameObject healthHolder;
    public bool isThrottleBar = false;
    public bool isFuelBar = false;
    public Slider slider;

    // Start is called before the first frame update
    void Start()
    {

    }

    void Update()
    {
        if (isThrottleBar)
        {
            slider.value = healthHolder.GetComponent<ShipController>().throttle / healthHolder.GetComponent<ShipController>().maxThrottle;
        }
        /*
        else if (isFuelBar)
        {
            slider.value = healthHolder.GetComponent<ShipController>().fuel / healthHolder.GetComponent<ShipController>().fuelCapacity;
        }
        */
    }
}
