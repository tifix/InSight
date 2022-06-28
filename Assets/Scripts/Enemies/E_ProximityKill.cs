using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class E_ProximityKill : E_Behaviour
{
    [Tooltip("when enemy is tracking, and gets this close, they will kill the player")] public float contactDamageRange = 2.5f;

    public override void LoopedAction() => CheckProximity(contactDamageRange);


    // Update is called once per frame
    void CheckProximity(float range)
    {
        foreach (Collider col in Physics.OverlapSphere(transform.position,range))
        {
            if (col.CompareTag("Player")) PlayerInRange();
        }
    }

    void PlayerInRange() 
    {
        Debug.Log("Player now in range");
        enemy_core.detector.KillInRange.Invoke();
    }
}
