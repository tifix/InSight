using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Det_Visual : Detector
{
    [Tooltip("Sensor triggered by player")] public Sensor vision_cone;

    public override void WhenDetecting() 
    {
        base.WhenDetecting();

        if (detection_state == det_states.undetected || detection_state == det_states.suspected)
        {
            detection_state = det_states.suspected;
            if (!Player.instance.pDetection.isDetectionGainFrozen) cur_detection += Player.instance.pDetection.mulVisualCur * Time.fixedDeltaTime * detGain;
        }
        //when tracking, build up additional detection points 
        if (detection_state == det_states.tracked)
        {
            if (!Player.instance.pDetection.isDetectionGainFrozen) cur_detection += Player.instance.pDetection.mulVisualCur * Time.fixedDeltaTime * detGainTracked;
        }

        //upon reaching detection threshhold - set status to detecting
        if (detection_state != det_states.detected && cur_detection >= detToSpot)
        {
            Debug.LogWarning("Intruder!");
            NowDetected.Invoke();
            UI_Handler.instance.SetDetectionColorDet();
            DataHolder.instance.AddDetection();
            detection_state = det_states.detected;
        }
        

        //last known location updating when player is seen
        if ((detection_state == det_states.detected || detection_state == det_states.tracked) && cur_detection >= detToSpot) lastPlayerLocation = Player.instance.transform.position;
    }



    //returns true if player is in cone of vision and not behind cover, otherwise returns false.
    public override bool CheckIfDetecting()         
    {
        if (vision_cone.detecting)
        {
            int mask = LayerMask.GetMask("Enemy", "EnemySense", "Smell", "Objective");
            mask = ~mask;

            if (RaycastToPlayer(mask))
            {
                return true;
            }

        }
        return false;
    }
}
