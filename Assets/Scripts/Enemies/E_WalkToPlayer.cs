using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class E_WalkToPlayer : E_Behaviour
{
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
        enemy_core.detector.agent.SetDestination(enemy_core.detector.last_player_location);
    }
}
