using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Det_Smell : Detector
{
    [Header("Smell specific params")]
                                                            public float            smelling_radius = 7f;
                                                            public float            freshnessDetectMargin = 0.05f;
                                                                   SphereCollider   smelling_sphere;
    [Tooltip("Sensor triggered by player")]                 public SensorFiltering  nose;
    [Tooltip("Should detection range be offset by wind?")]  public bool             isWindAffected = true;


    #region monobehaviourintegrations
    protected override void Start()
    {
        base.Start();
        smelling_sphere = nose.gameObject.GetComponent<SphereCollider>();
        smelling_sphere.radius = smelling_radius;
    }

    public override void FixedUpdate()
    {
        if(detection_state!=det_states.detected)


            if (cur_detection > Mathf.Epsilon) cur_detection= Mathf.Lerp(cur_detection- detDecay * Time.fixedDeltaTime,0,100);

        if (isWindAffected) OffsetSmellByWind();
        base.FixedUpdate(); //Base handles behaviour when not detecting
    }
    #endregion


    public override bool CheckIfDetecting()
    {
        if (nose.detecting)    //&& detection_state != det_states.detected
        {
            return true;
            /*
            //if I smell something, I stop patrolling and looking around and start tracking
            if (TryGetComponent<E_FollowScent>(out E_FollowScent fs)) if (!fs.is_performing) fs.TogglePerforming(true);
            if (TryGetComponent<E_PatrolArea>(out E_PatrolArea pa)) if (pa.is_performing) pa.TogglePerforming(false);
            if (TryGetComponent<E_LookAround>(out E_LookAround la)) if (la.is_performing) la.TogglePerforming(false);
            return true;
            */
        }
        else //if (detection_state != det_states.detected)
        {
            return false;
            /*
            //if I don't smell anything, I stop tracking, and start idling
            if (TryGetComponent<E_FollowScent>(out E_FollowScent fs)) if (fs.is_performing) fs.TogglePerforming(false);
            if (TryGetComponent<E_PatrolArea>(out E_PatrolArea pa)) if (!pa.is_performing) pa.TogglePerforming(true);
            if (TryGetComponent<E_LookAround>(out E_LookAround la)) if (!la.is_performing) la.TogglePerforming(true);
            return false;

            */
        }
        //else return true;
    }
    public override void WhenDetecting()
    {
        if (detection_state == det_states.undetected || detection_state == det_states.suspected)
        {
            if (nose.allDetected[0] != null) 
            {
            detection_state = det_states.tracked;
            GetComponent<EnemyCore>().SwitchToTracking();
            }

        }
        
        //upon reaching detection threshhold - set status to detecting
        if (detection_state != det_states.detected && cur_detection >= detToSpot)
        {
            Debug.LogWarning("Intruder!");
            NowDetected.Invoke();
            UI_Handler.instance.SetDetectionColorDet();
            GameManager.instance.AddDetection();
            detection_state = det_states.detected;
        }
        

        //last known location updating when player is seen
        if ((detection_state == det_states.detected || detection_state == det_states.tracked) && cur_detection >= detToSpot) 
            lastPlayerLocation = Player.instance.transform.position;
        
    }




    public void OffsetSmellByWind() 
    {
        smelling_sphere.transform.position = transform.position;
        smelling_sphere.center=-Weather.wind/2;     //direction inverted, as the smell travels FROM the track TO the detector
    }

}
