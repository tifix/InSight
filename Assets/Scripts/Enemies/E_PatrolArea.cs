using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class E_PatrolArea : E_Behaviour
{
    [Header("Patroling specific params")]
    [SerializeField] private MeshCollider   patrol_zone;
    [SerializeField] private NavMeshAgent   agent;
    [SerializeField] private Vector3        destination;

    public override void LoopedAction() => DeterminePatrolDestination();



    public void OnDrawGizmos()
    {
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(destination, 0.5f);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q) && DataHolder.instance.cheats.devToolsEnabled) SetDestinationToMousePosition();  //bait enemies with 'Q'
    }

    Vector3 DeterminePatrolDestination() 
    {
        Vector3 ret=Vector3.zero;
        int timeout = 0;
        while (timeout<50)
        {
            timeout++;
            bool is_viable = true;
            /*
             * 
             * unflatten destination sampling
            */

            destination = new Vector3(Random.Range(patrol_zone.bounds.min.x, patrol_zone.bounds.max.x), 
                                        Random.Range(patrol_zone.bounds.min.y, patrol_zone.bounds.max.y), 
                                            Random.Range(patrol_zone.bounds.min.z, patrol_zone.bounds.max.z));

            //destination would not be viable if point is generated in a wall, or on a botomless pit
            LayerMask mask = LayerMask.GetMask("Terrain");
            Collider[] overlappedcols = Physics.OverlapSphere(destination, 0.1f, mask);

            if (overlappedcols.Length < 1) { is_viable = false; }

            //destination would not be viable if cannot navigate to it
            if (is_viable)
            {
                agent.SetDestination(destination);
                if (agent.pathStatus == NavMeshPathStatus.PathInvalid) { is_viable = false; Debug.Log("not reachable"); continue; }
            }
            if (is_viable) break;   //repeating generation until a suitable point is found
        }
        if(timeout>10) Debug.Log(transform.parent.name+" has found destination in "+ timeout+ " tries");
        return destination;
    }

    void SetDestinationToMousePosition()
    {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit)) agent.SetDestination(hit.point);
    }

}
