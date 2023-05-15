using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class E_PatrolPoints : E_Behaviour
{
    public List<Transform> points;
    public int curPoint=0;
    public float speedMultiplier=1;
    //public float waitAtDestination = 15;

    public override void LoopedAction() => WalkToPlayerPos();

    void WalkToPlayerPos() 
    {
        curPoint++;
        if (curPoint > points.Count - 1) curPoint = 0;

        enemy_core.detector.agent.SetDestination(points[curPoint].position);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        for (int i = 0; i < points.Count; i++)
        {
            Gizmos.DrawSphere(points[i].position, 0.5f);
            try { Gizmos.DrawLine(points[i].position, points[i + 1].position); }
            catch { Gizmos.DrawLine(points[i].position, points[0].position); }
        }
        Gizmos.DrawCube(points[curPoint].position, Vector3.one*0.7f);
    }
}
