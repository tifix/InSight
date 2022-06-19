using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Det_Visual : Detector
{
    public Sensor vision_cone;

    public override void WhenDetecting() 
    {    
        if (detection_state == det_states.undetected || detection_state == det_states.suspected)
        {
            detection_state = det_states.suspected;
            cur_detection += Player.instance.pDetection.mulVisualCur * Time.fixedDeltaTime * detection_gain_rate;
        }
        //when tracking, build up additional detection points 
        if (detection_state == det_states.tracked)
        {
            cur_detection += Player.instance.pDetection.mulVisualCur * Time.fixedDeltaTime * detection_tracked_gain_rate;
        }

        //upon reaching detection threshhold - set status to detecting
        if (detection_state != det_states.detected && cur_detection > detection_to_sound_alarn)
        {
            Debug.LogWarning("Intruder!");
            pl_detected.Invoke();
            UI_Handler.instance.SetDetectionColorDet();
            DataHolder.instance.AddDetection();
            detection_state = det_states.detected;
        }
        

        //last known location updating when player is seen
        if ((detection_state == det_states.detected || detection_state == det_states.tracked) && cur_detection > detection_to_sound_alarn) last_player_location = Player.instance.transform.position;
    }




    public override bool CheckIfDetecting()         //returns true if player is in cone of vision and not behind cover, otherwise returns false.
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
