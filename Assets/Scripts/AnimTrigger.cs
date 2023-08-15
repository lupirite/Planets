using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimTrigger : MonoBehaviour
{
    public Animator animController;
    public string v;
    
    public void pressButton()
    {
        animController.SetBool(v, !animController.GetBool(v));
    }
}