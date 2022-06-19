using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class E_FollowScent : E_Behaviour
{
    public SmellNode freshest_track;


    static int SortByExpiry(SmellNode s1, SmellNode s2)    //in  a list of detectors, use this comparer to get the freshest track to [0]
    {
        return s1.perc_passed.CompareTo(s2.perc_passed);
    }

    public override void LoopedAction()
    {
        //refresh tracks
        freshest_track = RefreshTracks();

        if (freshest_track == null) 
        {
            Debug.Log("no track found, aborting");
            return;
        }

        enemy_core.detector.agent.SetDestination(freshest_track.transform.position);
        float det_increase = (1f - freshest_track.perc_passed) * enemy_core.detector.detection_gain_rate * Player.instance.pDetection.mulSmellCur;    
        Debug.Log(det_increase);
        enemy_core.detector.cur_detection = Mathf.Clamp(enemy_core.detector.cur_detection + det_increase, 0, 100);
    }


    private SmellNode RefreshTracks() //sorts and returns freshest track
    {
        for (int i = enemy_core.GetComponent<Det_Smell>().nose.allDetected.Count - 1; i > -1; i--)
        {
            //if I see a track that's been expired, I tell myself it's no longer there
            if (enemy_core.GetComponent<Det_Smell>().nose.allDetected[i] == null) 
            { 
                enemy_core.GetComponent<Det_Smell>().nose.allDetected.RemoveAt(i); 
                continue; 
            }
        }
        enemy_core.GetComponent<Det_Smell>().nose.allDetected.Sort(SortByExpiry);
        try 
        {
            Debug.Log("out of " + enemy_core.GetComponent<Det_Smell>().nose.allDetected.Count + " tracks this one is the freshest>>" + 
            enemy_core.GetComponent<Det_Smell>().nose.allDetected[0].name + " is:  " + 
            enemy_core.GetComponent<Det_Smell>().nose.allDetected[0].perc_passed);

            return enemy_core.GetComponent<Det_Smell>().nose.allDetected[0];
        }
        catch 
        {
            Debug.LogWarning("No tracks detected. Proceed to undetected");
            return null;
        }

        
    }

      private void OnDrawGizmos()
    {
        if (freshest_track != null) { Gizmos.color = Color.red; Gizmos.DrawCube(freshest_track.transform.position, Vector3.one); }
        
    }

}
