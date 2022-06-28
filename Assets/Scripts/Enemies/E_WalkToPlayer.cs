using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class E_WalkToPlayer : E_Behaviour
{
    public float speedMultiplier=1;
    /*
    public const behaviourType type = behaviourType.walk;

    public E_WalkToPlayer() 
    {
        name = "Walking behaviour";
    }
    */

    public override void LoopedAction() => WalkToPlayerPos();

    void WalkToPlayerPos() 
    {
        enemy_core.detector.agent.SetDestination(enemy_core.detector.lastPlayerLocation);
    }
}
