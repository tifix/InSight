using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class SmartFoliageSpawner : MonoBehaviour
{
    [Tooltip("Leave ticked for refreshing entities - uncheck to obliterate instantly."), SerializeField] bool keepAlive= true;
    private Ray sufaceNormal, sufaceNormalB;
    [Tooltip("Tick to snap to nearest flat surface"), SerializeField]  bool snapToNormal = false;
    [Tooltip("Tick to snap to nearest flat surface"), SerializeField]  bool showNormalGizmo = true;
    [Tooltip("Tick to snap to nearest flat surface"), SerializeField]  bool randomizeRotationAroundPrimary = false;
    [Tooltip("Tick to snap to nearest flat surface"), SerializeField]  bool showRotationlGizmo = false;
    public Vector3 debugPosition= Vector3.zero;
    [Range(0f,5f)] public float SnapMaxDistance = 5;
    float shortestSnapDistance = 5;

    void Awake()
    {
        
        //sufaceNormal = 
        /*
        if (sprite == null) //Safeguard for automatic assigning with safeguard against no SpriteRenderer
        {
            if (TryGetComponent<SpriteRenderer>(out SpriteRenderer SR)) sprite = SR;
            else Debug.LogWarning("Sprite Ordering script on object <"+ gameObject.name + "> which has no sprite renderer!");
        }

        sprite.sortingOrder = (int)transform.position.z;*/
    }

    private void OnDisable()
    {
        DestroyImmediate(this);
    }

    void Update()
    {
        SnapToNormal();
        RandomiseRotaion();

        if (!keepAlive) DestroyImmediate(this);              //For static objects, the layer shouldn't update for performance sake - destroy script once redundant
    }

    void SnapToNormal() 
    {
        shortestSnapDistance = SnapMaxDistance;
        //Cast for snap distance flush
        if (Physics.Raycast(new Ray(transform.position, -transform.forward), out RaycastHit hitZero, SnapMaxDistance))
        {
            if (hitZero.distance <= shortestSnapDistance) { shortestSnapDistance = hitZero.distance; }
        }

        //Snap forward
        if (Physics.Raycast(new Ray(transform.position, -transform.forward), out RaycastHit hitForward, SnapMaxDistance))
        {
            if (hitForward.distance <= shortestSnapDistance)
            {
                shortestSnapDistance = hitForward.distance;
                sufaceNormal = new Ray(transform.position, hitForward.normal);
                debugPosition = hitForward.point;
            }
        }
        //Snap anti up
        if (Physics.Raycast(new Ray(transform.position, -transform.up), out RaycastHit hitUp, shortestSnapDistance))
        {
            if (hitUp.distance <= shortestSnapDistance)
            {
                shortestSnapDistance = hitUp.distance;
                sufaceNormalB = new Ray(transform.position, hitUp.normal);
                debugPosition = hitUp.point;
            }
        }
        //Snap up
        if (Physics.Raycast(new Ray(transform.position, transform.up), out RaycastHit hitAntiUp, shortestSnapDistance))
        {
            if (hitAntiUp.distance <= shortestSnapDistance)
            {
                shortestSnapDistance = hitAntiUp.distance;
                sufaceNormalB = new Ray(transform.position, hitAntiUp.normal);
                debugPosition = hitAntiUp.point;
            }
        }


        if (snapToNormal && hitForward.distance <= shortestSnapDistance)
        {
            transform.rotation = Quaternion.LookRotation(hitForward.normal, transform.up); //Preserving previous rotation

            transform.position = debugPosition;
            Debug.Log("Snapping forward Now!");
            snapToNormal = false;
        }
        else if (snapToNormal && hitUp.distance <= shortestSnapDistance)
        {
            //transform.up = hitB.normal;
            transform.rotation = Quaternion.LookRotation(transform.forward, hitUp.normal);
            transform.position = debugPosition;
            Debug.Log("Snapping anti up Now!");
            snapToNormal = false;
        }
        else if (snapToNormal && hitAntiUp.distance <= shortestSnapDistance)
        {
            //transform.up = -hitAntiUp.normal;
            transform.rotation = Quaternion.LookRotation(transform.forward, -hitAntiUp.normal);
            transform.position = debugPosition;
            Debug.Log("Snapping up Now!");
            snapToNormal = false;
        }

    }

    void RandomiseRotaion()
    {
        /*
            if (Vector3.Angle(transform.up, Vector3.up) < 30)  //up is primary
            {
                if (showRotationlGizmo) Debug.DrawRay(transform.position, transform.up);

                if (randomizeRotationAroundPrimary)
                { transform.rotation = Quaternion.LookRotation(transform.forward * Random.Range(-1, 1) + transform.right * Random.Range(-1, 1), transform.up); randomizeRotationAroundPrimary = false; }
                
            }  */

        if (Vector3.Angle(transform.forward, Vector3.up) < 30)  //forward is primary
        {
            if (showRotationlGizmo) Debug.DrawRay(transform.position, transform.forward);

            if (randomizeRotationAroundPrimary)
            { transform.rotation = Quaternion.LookRotation(transform.forward, transform.up*Random.Range(-1f,1f)+transform.right*Random.Range(-1f, 1f)); randomizeRotationAroundPrimary = false; }


            //float correctestAxis

            //Vector3.Angle()
        }
    }

        private void OnDrawGizmosSelected()
    {
        if (showNormalGizmo) 
        {
            if (debugPosition != Vector3.zero)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawSphere(debugPosition, 0.12f);

                Gizmos.color = Color.blue; Gizmos.DrawRay(debugPosition, sufaceNormal.direction);
                Gizmos.color = Color.green; Gizmos.DrawRay(debugPosition, sufaceNormalB.direction);
            }
        }
        
        
    }
}
