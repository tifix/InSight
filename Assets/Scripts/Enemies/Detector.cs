using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

public class Detector : MonoBehaviour
{
    ///  test testoo testaroonie aoinfoadfboaboabvofvfvfvafvcvxcvxccvwrqefsdf
    /// 
    /// asd
    /// asd
    /// 
    /// asv
    /// 
    /// </summary>

    //Event triggers for state switching
    [HideInInspector] public UnityEvent StartSuspecting = new UnityEvent();
    [HideInInspector] public UnityEvent NowDetected     = new UnityEvent();
    [HideInInspector] public UnityEvent StartTracking   = new UnityEvent();
    [HideInInspector] public UnityEvent GiveUpTracking  = new UnityEvent();
    [HideInInspector] public UnityEvent KillInRange     = new UnityEvent();

    //Detection essentials
                      public det_states detection_state = det_states.undetected;
                      public float      cur_detection, remainingTrackingTime;
                      public float      distancePlayer;
    [SerializeField]         Transform  detectionSource;
    
    [Space][Header("Balancing parameters")]
    public          float detDecay       = 20;
    public          float detGain        = 25;
    public          float detGainTracked = 40;
    public readonly float detToSpot      = 100;
    [Space]
    //
    [Tooltip("Time between enemy fully spotting the player and them engaging")]         public          float detShockLength = 0.5f;  //= 0.5f;
    [Tooltip("Safety check to ensure only 1 detection happens in a short timespan")]    public          bool currentlyEngaging { get; set; } = false;
    [Tooltip("time spent lookin after player is no longer detected")]                   public          float trackingTime = 10;
    

    [Tooltip("the last known position of the player")]                                  public          Vector3 lastPlayerLocation;
    public enum det_states {undetected,suspected, detected, tracked}
    [HideInInspector] public NavMeshAgent agent;

    #region monobehaviour integrations

    protected virtual void Start()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    public virtual void FixedUpdate()
    {
        cur_detection = Mathf.Clamp(cur_detection, 0, 100);

        //if player is not currently sensed
        if (CheckIfDetecting()) WhenDetecting();
        else WhenNotDetecting();                 
    }

    private void OnDrawGizmosSelected()
    {
        if (detection_state == det_states.suspected)
        {
            Color debug = Color.Lerp(Color.yellow, Color.red, cur_detection / 100f);
            Gizmos.color = debug;
            Gizmos.DrawCube(transform.position, Vector3.one * 0.5f);
            Gizmos.DrawCube(Player.instance.transform.position, Vector3.one * 0.5f);
            Gizmos.DrawLine(transform.position, Player.instance.transform.position);
        }
        if (detection_state == det_states.tracked)
        {
            Gizmos.color = Color.black;
            Gizmos.DrawCube(transform.position, Vector3.one * 0.5f);
            Gizmos.DrawCube(Player.instance.transform.position, Vector3.one * 0.5f);
            Gizmos.DrawLine(transform.position, Player.instance.transform.position);
        }
        //if(detection_state==det_states.suspected || detection_state == det_states.tracked)
    }
    #endregion



    public virtual void WhenNotDetecting() 
    {
        if (detection_state == det_states.detected)
        {
            remainingTrackingTime = trackingTime;
            detection_state = det_states.tracked;

            //Start tracking HERE
            lastPlayerLocation = Player.instance.transform.position;
            StartTracking.Invoke();
            UI_Handler.instance.SetDetectionColorTrack();
        }
        else
        {
            //Detection slowly decaying , slower if tracked
            if (cur_detection > Mathf.Epsilon && detection_state == det_states.suspected) cur_detection -= detDecay * Time.deltaTime;
            else if (cur_detection > Mathf.Epsilon && detection_state == det_states.tracked) cur_detection -= detDecay / 2 * Time.deltaTime;

            //stopping suspection if detection is 0
            if (cur_detection <= Mathf.Epsilon && detection_state == det_states.suspected) { detection_state = det_states.undetected; cur_detection = 0; }

            if (detection_state == det_states.tracked)
            {
                if (remainingTrackingTime > Mathf.Epsilon) remainingTrackingTime -= Time.fixedDeltaTime; //count down time for tracking
                else { remainingTrackingTime = 0; detection_state = det_states.undetected; GiveUpTracking.Invoke();  }   //Stop tracking after time expires
            }
        }
    }


    public virtual void WhenDetecting()
    {
        if (detection_state == det_states.undetected) StartSuspecting.Invoke();
   
        //if (detection_state != det_states.detected && cur_detection == 100) NowDetected.Invoke();
    }



    protected bool RaycastToPlayer() 
    {
        int mask = LayerMask.GetMask("Enemy", "EnemySense", "Smell", "Objective", "Ignore Raycast");
        mask = ~mask;
        return RaycastToPlayer(mask);
    }

    //When in Cone of vision, raycast to check if behind cover or not. Returns false if player is behind cover
    protected bool RaycastToPlayer(int mask)
    {
        Ray ray = new Ray(detectionSource.position, Player.instance.transform.position - detectionSource.position);

        if (Physics.Raycast(ray.origin, ray.direction, out RaycastHit hit, 2000, mask))
        {
            //Debug.DrawLine(ray.origin, hit.point, Color.green, 0.1f);
            if (hit.transform.CompareTag("Player")) return true;
        }
        return false;

    }   


    public virtual bool CheckIfDetecting()     //to be overwritten by actual senses
    {
        return false;
    }

}
