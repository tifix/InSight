using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Det_Audio : Detector
{
    [Tooltip("Sensor triggered by player")] public  Sensor            ears;
    [Header("Hearing specific parameters")]
                                            public  float             player_noise=2;
                                            public  float             detGainPerStep=0;
                                            public  bool              isUsingComparativeVolume = false;
                                            public  float             noiseDif = 0;

                                            public  List<AudioEntity> AudioEntities;
                                            public  float             hearing_radius = 12.5f;
                                            private SphereCollider    hearing_sphere;

    [System.Serializable]
    public class AudioEntity
    {
        public float volumeAbsolute = 1;
        public float volumeRelative = 1;
        public Transform AudioObject;
    }


    protected override void Start()
    {
        base.Start();
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

    // ambient noise detection
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

    public void ListenToPlayerSteps() 
    {
        if (distancePlayer< hearing_radius)
            if (detection_state == det_states.undetected || detection_state == det_states.suspected)
            {
                if (RefreshAudioEntities().CompareTag("Player")) 
                {
                    detection_state = det_states.suspected;
                    
                    //Comparative volume system- if player is only a bit louder than the environment, then the detection increaase is slower
                    noiseDif = Mathf.Clamp( AudioEntities[0].volumeRelative - AudioEntities[1].volumeRelative,0,1.5f);

                    detGainPerStep = 0;
                    
                    if (isUsingComparativeVolume) detGainPerStep = AudioEntities[0].volumeRelative * detGain * noiseDif * Player.instance.pDetection.mulAudioCur;
                    else detGainPerStep = AudioEntities[0].volumeRelative * detGain;

                    if (detGainPerStep > 33) Debug.Log("making extreme noise "+ detGainPerStep);
                    if(!Player.instance.pDetection.isDetectionGainFrozen)cur_detection += Mathf.Clamp(detGainPerStep, 0, 33); 
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

            if (!Player.instance.pDetection.isDetectionGainFrozen) cur_detection += Mathf.Clamp(detGainPerStep, 0, 33);

        }

    }

    public override void WhenDetecting()
    {
        base.WhenDetecting();
        /*Detection buildup in ListenToPlayerSteps
         */

        //upon reaching detection threshhold - set status to detecting
        if (detection_state != det_states.detected && cur_detection >= detToSpot)
        {
            Debug.LogWarning("Intruder!");
            NowDetected.Invoke();
            UI_Handler.instance.SetDetectionColorDet();
            DataHolder.instance.AddDetection();
            detection_state = det_states.detected;
        }

        //last known location updating when player is detected
        if((detection_state==det_states.detected || detection_state==det_states.tracked)&& cur_detection >= detToSpot) lastPlayerLocation = Player.instance.transform.position;   
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
