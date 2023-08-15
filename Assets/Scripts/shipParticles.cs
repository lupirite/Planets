using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class shipParticles : MonoBehaviour
{
    public Transform chair;
    public ParticleSystem ps;

    public AnimationCurve particleSpeedCurve;

    void Update()
    {
        if (chair.childCount > 0)
        {
            if (!ps.isPlaying)
            {
                ps.Play();
            }
            var main = ps.main;
            ShipController sc = chair.Find("Player").GetComponent<ShipController>();
            if (sc.throttle > 0)
                main.simulationSpeed = particleSpeedCurve.Evaluate(sc.throttle / sc.maxThrottle);
            else
                ps.Stop();
        }
        else
        {
            ps.Stop();
        }
    }
}
