using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]

public class E_VibeCheck : E_Behaviour
{
    public float speedMultiplier = 1;
    [SerializeField] float leapDuration = 3;
    [SerializeField] float MaxLeapDistance = 1;
    [SerializeField] bool isLeaping = false;
    [SerializeField] AnimationCurve ArcProfile;

    public override void LoopedAction() => WalkAndJump();
    void WalkAndJump()
    {
        Ray temp = new Ray(transform.position, Player.instance.transform.position+Vector3.up*0.5f - transform.position);
        Debug.DrawLine(transform.position, transform.position + temp.direction * MaxLeapDistance,Color.red);
        if(!isLeaping) enemy_core.detector.agent.SetDestination(enemy_core.detector.lastPlayerLocation);

        int mask = LayerMask.GetMask("Enemy", "EnemySense", "Smell", "Objective", "Water", "Ignore Raycast");
        mask = ~mask;
        if (!isLeaping && Physics.Raycast(temp, out RaycastHit hit, MaxLeapDistance,mask))
        {
            if (Player.instance.pDetection.isInSafeSpace) return;   //DO NOT LEAP if player is in a safespace
            Debug.Log("hit: "+hit.collider.gameObject.name);
            if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Player")) 
            {
                Debug.Log("Hit player indeed!");
                enemy_core.StartCoroutine(LeapAtPlayer());
            }
        }
    }
    IEnumerator LeapAtPlayer() 
    {
        isLeaping = true;
        GetComponent<NavMeshAgent>().enabled = false;
        Player.instance.isMovementLocked = true;
        float i = 0;
        Vector3 initPos = transform.position;
        while (i<1)
        {
            transform.position = Vector3.Lerp(initPos, Player.instance.transform.position+Vector3.up* ArcProfile.Evaluate(i), i);
            yield return new WaitForFixedUpdate();
            i += Time.fixedDeltaTime/ leapDuration;
        }
        Player.instance.Die("You've been Leaped");
    }
}
