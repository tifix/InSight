using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Det_Audio : Detector
{
    public Sensor ears;
    [Header("Hearing specific parameters")]

    [SerializeField] private float distancePlayer;
    public float player_noise=2;
    public List<AudioEntity> AudioEntities;

    public float hearing_radius = 12.5f;
    private SphereCollider hearing_sphere;

    [System.Serializable]
    public class AudioEntity
    {
        public float volumeAbsolute = 1;
        public float volumeRelative = 1;
        public Transform AudioObject;
    }


    public void Start()
    {
        hearing_sphere = ears.gameObject.GetComponent<SphereCollider>();
        hearing_sphere.radius = hearing_radius;

        //listener for player steps
        Player.instance.pDetection.StepTaken.AddListener(ListenToPlayerSteps);
    }



    public virtual GameObject RefreshAudioEntities()
    {

        foreach (var item in AudioEntities) 
        {
            if (item.AudioObject.CompareTag("Player")) //Refreshing player step volume
            {
                player_noise = Player.instance.pDetection.mulAudioCur;
                if (RaycastToPlayer() == false) player_noise *= 0.85f;
                item.volumeAbsolute = player_noise;
            }
        }

        if (AudioEntities.Count > 1) AudioEntities.Sort(SortByNoise);
        return AudioEntities[0].AudioObject.gameObject;
    }

    #region ambient noise detection

    private int SortByNoise(AudioEntity s1, AudioEntity s2)
    {
        // if (s1.AudioObject.gameObject.TryGetComponent<AudioSource>(out AudioSource source1))
        //if (s2.AudioObject.gameObject.TryGetComponent<AudioSource>(out AudioSource source2)) 
        {
            float d1 = (s1.AudioObject.position - transform.position).magnitude;
            float d2 = (s2.AudioObject.position - transform.position).magnitude;

            //logarythmic volume falloff 
            d1 = Mathf.Log(d1, 0.1f) + 1.7f;
            d2 = Mathf.Log(d2, 0.1f) + 1.7f;
            s1.volumeRelative =Mathf.Clamp(s1.volumeAbsolute * d1, 0,20)  ;
            s2.volumeRelative = Mathf.Clamp(s2.volumeAbsolute * d2, 0, 20);

            return (s2.volumeRelative).CompareTo(s1.volumeRelative);

        }

        //Debug.Log("Sorting by audio contains a an object without an audio source!");
        //return 0;// s2.name.Length.CompareTo(s1.name.Length);    //if 
    }
    #endregion

    public void ListenToPlayerSteps() 
    {
        if (distancePlayer< hearing_radius)
            if (detection_state == det_states.undetected || detection_state == det_states.suspected)
            {
                if (RefreshAudioEntities().CompareTag("Player")) 
                {
                    detection_state = det_states.suspected;
                    cur_detection += AudioEntities[0].volumeRelative * Time.fixedDeltaTime * detection_gain_rate;
                }
            }

        //when tracking,  build up additional detection points
        if (detection_state == det_states.tracked)
        {
            RefreshAudioEntities();
            float PlayerNoiseCur = 0;
            foreach (var item in AudioEntities)
            {
                if (item.AudioObject.CompareTag("Player")) PlayerNoiseCur = item.volumeRelative;
            }
            
            cur_detection += PlayerNoiseCur * Time.fixedDeltaTime * detection_gain_rate;
            
        }

    }

    public override void WhenDetecting()
    {
        /*Detection buildup in ListenToPlayerSteps
         */

        //upon reaching detection threshhold - set status to detecting
        if (detection_state != det_states.detected && cur_detection > detection_to_sound_alarn)
        {
            Debug.LogWarning("Intruder!");
            pl_detected.Invoke();
            UI_Handler.instance.SetDetectionColorDet();
            DataHolder.instance.AddDetection();
            detection_state = det_states.detected;
        }

        //last known location updating when player is detected
        if((detection_state==det_states.detected || detection_state==det_states.tracked)&& cur_detection>detection_to_sound_alarn) last_player_location = Player.instance.transform.position;   
    }

    //returns true if player is in cone of vision and not behind cover, otherwise returns false.
    public override bool CheckIfDetecting()
    {
        if (ears.detecting)
        {
            distancePlayer=(Player.instance.transform.position-transform.position).magnitude;

            //if player is the loudest of the heard sound entities, he is detected, otherwise, enemy is oblivious
            if (RefreshAudioEntities().CompareTag("Player")) return true;
            else return false;
        }
        return false;
    }
}
