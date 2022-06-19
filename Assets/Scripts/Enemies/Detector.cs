using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

public class Detector : MonoBehaviour
{
    [HideInInspector] public UnityEvent pl_detected=new UnityEvent();
    [HideInInspector] public UnityEvent pl_start_tracking=new UnityEvent();
    [HideInInspector] public UnityEvent pl_end_tracking = new UnityEvent();

    public det_states detection_state = det_states.undetected;
    public float cur_detection, remaining_tracking_time;
    public Transform detectionSource;

    [Space]
    [Header("Balancing parameters")]
    public float detection_to_sound_alarn = 100;
    public float detection_decay_rate = 20;
    public float detection_gain_rate = 25;
    public float detection_tracked_gain_rate = 40;

    [Tooltip("time spent lookin after player is no longer detected")]   public float tracking_time = 10;
    [Tooltip("the last known position of the player")] public Vector3 last_player_location;

    public enum det_states {undetected,suspected,tracked,detected}

    [Header("Connected objects")]
    public NavMeshAgent agent;

    public virtual void FixedUpdate()
    {
        //if player is not currently sensed
        if (CheckIfDetecting()) WhenDetecting();
        else WhenNotDetecting();
                           
    }
    public virtual void WhenNotDetecting() 
    {
        if (detection_state == det_states.detected)
        {
            remaining_tracking_time = tracking_time;
            detection_state = det_states.tracked;

            //Start tracking HERE
            last_player_location = Player.instance.transform.position;
            pl_start_tracking.Invoke();
            UI_Handler.instance.SetDetectionColorTrack();
        }
        else
        {
            //Detection slowly decaying , slower if tracked
            if (cur_detection > Mathf.Epsilon && detection_state == det_states.suspected) cur_detection -= detection_decay_rate * Time.deltaTime;
            else if (cur_detection > Mathf.Epsilon && detection_state == det_states.tracked) cur_detection -= detection_decay_rate / 2 * Time.deltaTime;

            //stopping suspection if detection is 0
            if (cur_detection <= Mathf.Epsilon && detection_state == det_states.suspected) { detection_state = det_states.undetected; cur_detection = 0; }

            if (detection_state == det_states.tracked)
            {
                if (remaining_tracking_time > Mathf.Epsilon) remaining_tracking_time -= Time.fixedDeltaTime; //count down time for tracking
                else { remaining_tracking_time = 0; detection_state = det_states.undetected; pl_end_tracking.Invoke(); UI_Handler.instance.SetDetectionColorSus(); }   //Stop tracking after time expires
            }
        }
    }
    public virtual void WhenDetecting()
    {

    }

    protected bool RaycastToPlayer() 
    {
        int mask = LayerMask.GetMask("Enemy", "EnemySense", "Smell", "Objective");
        mask = ~mask;
        return RaycastToPlayer(mask);
    }

    //When in Cone of vision, raycast to check if behind cover or not. Returns false if player is behind cover
    protected bool RaycastToPlayer(int mask)
    {
        Ray ray = new Ray(detectionSource.position, Player.instance.transform.position - detectionSource.position);

        if (Physics.Raycast(ray.origin, ray.direction, out RaycastHit hit, 2000, mask))
        {
            Debug.DrawLine(ray.origin, hit.point, Color.green, 0.1f);
            if (hit.transform.CompareTag("Player")) return true;
        }
        return false;

    }   


    public virtual bool CheckIfDetecting()     //to be overwritten by actual senses
    {
        return false;
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawCube(last_player_location, Vector3.one*0.5f);
    }

}
