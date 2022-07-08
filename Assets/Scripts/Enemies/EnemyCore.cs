using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Detector))]

public class EnemyCore : MonoBehaviour
{
    [SerializeField]                                            private enemy_state     cur_state = enemy_state.idle;
    [Tooltip("What actions the AI can perform in what states")] public E_Behaviour[]    Behaviours;
    [Space]
    [HideInInspector]                                           public Detector         detector;
    [Tooltip("base movement speed, increased when tracking")]   public readonly float   moveSpeed = 3.5f;
                                                                public enum enemy_state {idle, suspicious, flashDetected,tracking, attacking, unloaded};

    #region monobehaviour integrations
    private void Awake()
    {
        detector = GetComponent<Detector>();
    }

    void Start()
    {
        GetComponent<NavMeshAgent>().speed = moveSpeed;
        detector.NowDetected.AddListener(TriggerDetection);

        detector.StartSuspecting.AddListener(SwitchToSus);
        detector.NowDetected.AddListener(SwitchToDetecting);
        detector.StartTracking.AddListener(SwitchToTracking);
        detector.GiveUpTracking.AddListener(SwitchToIdle);      detector.KillInRange.AddListener(SwitchToAttacking);
        detector.GiveUpTracking.AddListener(EndEngaging); 
        //StartCoroutine(UnloadIfVeryFar(200f, 1f));
    }

    private IEnumerator UnloadIfVeryFar(float distance, float interval) 
    {
        while (true) 
        {
            yield return new WaitForSecondsRealtime(interval);
            if (detector.distancePlayer > distance) { SwitchToUnloaded(); }
        }
    }

    #endregion

    //currentlyEngaging
    //detector.detection_state!=Detector.det_states.detected

    //Detection happens HERE!
    public void TriggerDetection() { if(!detector.currentlyEngaging) StartCoroutine(DetectedToTracked(detector.detShockLength)); }
    private IEnumerator DetectedToTracked(float shockPeriod)
    {
        detector.currentlyEngaging = true;
        detector.detection_state = Detector.det_states.detected;
        Debug.LogWarning("Detected!");
        yield return null;
        Player.instance.VFX_DetectFlash.Play();
        cur_state = enemy_state.flashDetected;
        yield return new WaitForSeconds(shockPeriod);
        detector.detection_state = Detector.det_states.tracked;
        cur_state = enemy_state.tracking;
        yield return null;
    }

    public void EndEngaging() 
    {
        detector.currentlyEngaging = false;
        detector.cur_detection = 0;
        SwitchToIdle();
    }

    void SwitchState(enemy_state target_state)
    {
        cur_state = target_state;

        foreach (E_Behaviour behaviour in Behaviours)
        {
            //Disabling expired behaviours
            if (behaviour.parent_state != cur_state && behaviour.is_performing) behaviour.TogglePerforming();

            //Starting newly appropriate behaviours
            if (behaviour.parent_state == cur_state && !behaviour.is_performing) behaviour.TogglePerforming();
        }
    }
    #region State switching shorthands
    public void SwitchToIdle() => SwitchState(enemy_state.idle);
    public void SwitchToSus() => SwitchState(enemy_state.suspicious);
    public void SwitchToDetecting() => SwitchState(enemy_state.flashDetected);
    public void SwitchToTracking() => SwitchState(enemy_state.tracking);
    public void SwitchToAttacking() => Player.instance.Die("getting gutted by " + name);  //=>SwitchState(enemy_state.attacking);
    public void SwitchToUnloaded() => SwitchState(enemy_state.unloaded);
    #endregion

}
