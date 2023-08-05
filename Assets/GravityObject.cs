using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GravityObject : MonoBehaviour
{
    public static List<GravityObject> gravityObjects = new List<GravityObject>();

    private void Start()
    {
        gravityObjects.Add(this);
    }
}
