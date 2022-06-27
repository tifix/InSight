using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Rigidbody))]
public class Player : MonoBehaviour
{
    [Header("Current")]
    public Move_mode current_movement = Move_mode.still;
    public bool isInWater = false;
    
    
 
    [System.Serializable] private class PlrSpeed
    {
        public Vector3 movement_direction;
        public Vector3 surfaceNormal;
        public float slope_limit = 30;

        public float movement_speed=60;
        public float sneak_speed =30;
        public float run_speed=100;
        public float fall_multiplier = 3;       //artificial snappy gravity
        public float jump_force= 20750;
        public float max_run_speed=13;
        public float max__speed=8;
        public float max_sneak_speed=3;
        public bool isLeftStepNow = false;
        public float step_offset = 0.2f;
    }
    [Tooltip("Speed parameters")][SerializeField] private PlrSpeed pMovement;

    [System.Serializable] public class PlrDetections
    {
        
        public float cur_Detection;
        public List<Detector> current_detectors = new();

        [Tooltip("current detection multipliers")]
        [Range(0, 4)]
        public float mulGlobal = 1;
        public float mulVisualCur = 1, mulAudioCur = 1, mulSmellCur = 1;                            //current detection multipliers
        public float mulVisualRun = 2f, mulVisualSneak = 0.3f;                                      //when running, detect faster, when sneaking - slower
        public float mulAudioStill = 0.2f, mulAudioSneak = 1f, mulAudioRun = 3.5f;                  //when running, detect faster, when sneaking- slower, when still- just barely
        public float intAudioSneak = 0.75f, intAudioWalk = 0.5455f, intAudioRun = 0.4286f;       //8-BPM  110BPM 140BPM
        public float intervalTrackSpawning=1f;
        public float stepInterval = 0.05f, stepLastTime = 0;
        [HideInInspector] public UnityEvent StepTaken;
    }
    [Tooltip("detection parameters")] public PlrDetections pDetection;

    public enum Move_mode { still, sneaking, walking, running}

    [Header("Related objects")]
    public Sensor_Player ground_sensor;
    public CameraController cam_cont;
    [HideInInspector] public Rigidbody rb;
    [SerializeField] private GameObject prefabTrack;
    [SerializeField] private GameObject prefabFootstep;
    public Transform enemy_holder;
    public Transform track_holder;
    public Transform last_checkpoint;
    [SerializeField] private Transform mesh_child;
    [SerializeField] private Transform head_child;
    public static Player instance;


    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    // Start is called before the first frame update
    void Start()
    {   
        StartCoroutine(LeaveBehindTracks());
    }

    // Update is called once per frame
    void Update()
    {
        //show the highest detection on detect slider
        UI_Handler.instance.detection_a.value = pDetection.cur_Detection;
        UI_Handler.instance.detection_b.value = pDetection.cur_Detection;

        //Mesh lil movements
        mesh_child.rotation = Quaternion.LookRotation(mesh_child.forward + VectorFlattenY(pMovement.movement_direction * 0.06f),Vector3.up);
        Quaternion q1 = Quaternion.LookRotation(head_child.forward + cam_cont.transform.right * 0.09f, Vector3.up);    //    + Camera.main.transform.forward*0.2f //pMovement.movement_direction 
        head_child.rotation = q1;
    }

    private void FixedUpdate()
    {
        MovementInput();
        ApplyMoveModes();
        NoiseOfSteps();

        UpdateDetectingEnemies();

        CheckIfFallen();
    }


    #region movement essentials
    
    void ApplyMoveModes() //applying sneak and run and walk and idle state effects
    {
        switch (current_movement)
        {
        case Move_mode.still:
            {
                pDetection.mulAudioCur = pDetection.mulAudioStill * pDetection.mulGlobal;
                pDetection.intervalTrackSpawning = 1/pDetection.mulGlobal;
                break;
            }
        case Move_mode.sneaking:
            {
                pDetection.mulVisualCur = pDetection.mulVisualSneak * pDetection.mulGlobal;
                pDetection.mulAudioCur = pDetection.mulAudioSneak * pDetection.mulGlobal;
                pDetection.intervalTrackSpawning = 1/ pDetection.mulGlobal;
                break;
            }
        case Move_mode.walking:
            {
                pDetection.mulVisualCur = 1 * pDetection.mulGlobal;    
                pDetection.mulAudioCur = 2f * pDetection.mulGlobal;
                    if(ground_sensor.detectingWater) pDetection.mulAudioCur *= 2f * pDetection.mulGlobal;
                pDetection.intervalTrackSpawning = 0.5f / pDetection.mulGlobal;
                break;
            }
        case Move_mode.running:
            {
                pDetection.mulVisualCur = pDetection.mulVisualRun * pDetection.mulGlobal;
                pDetection.mulAudioCur = pDetection.mulAudioRun * pDetection.mulGlobal;
                    if (ground_sensor.detectingWater) pDetection.mulAudioCur *= 2 * pDetection.mulGlobal;
                pDetection.intervalTrackSpawning = 0.25f / pDetection.mulGlobal;
                break;
            }
        default:
            {
                //passively return to  walking multiplier
                pDetection.mulVisualCur = 1 * pDetection.mulGlobal;
                pDetection.mulAudioCur = 2.5f * pDetection.mulGlobal;
                pDetection.intervalTrackSpawning = 0.5f / pDetection.mulGlobal;
                break;
            }   
        }    
    }

    public Vector3 GetSurfaceNormal() 
    {
        Ray r = new(mesh_child.position, -transform.up);
        
        if(Physics.Raycast(r, out RaycastHit hit, 250,LayerMask.GetMask("Terrain"))) 
        {
            Debug.DrawRay(transform.position + transform.up * 2, hit.normal * .3f, Color.green);
            if(Vector3.Angle(hit.normal, Vector3.up)>pMovement.slope_limit) return Vector3.up;

            return hit.normal;
        }
        return Vector3.up;
    }

    public Vector3 VectorFlattenY(Vector3 inVect3) 
    {
        return new Vector3(inVect3.x, 0, inVect3.z);
    }

    void MovementInput()
    {
        Vector2 InputVector = new Vector3(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        pMovement.surfaceNormal = GetSurfaceNormal();

        Vector3 MoveForward = Vector3.Cross(pMovement.surfaceNormal, cam_cont.transform.forward * InputVector.y).normalized;
        Vector3 MoveSideways = Vector3.Cross(pMovement.surfaceNormal, cam_cont.transform.right * InputVector.x).normalized;
        pMovement.movement_direction = MoveForward + MoveSideways;

        Debug.DrawRay(transform.position + transform.up*2, MoveForward*.3f, Color.blue);
        Debug.DrawRay(transform.position+transform.up * 2, MoveSideways * .3f, Color.red);
        Debug.DrawRay(transform.position+transform.up * 2, cam_cont.transform.right * .3f, Color.yellow);

        //applying the force depending on the current movement mode and maximum speed values
        Vector3 applied_force = Vector3.zero;
        if (current_movement == Move_mode.sneaking && rb.velocity.magnitude < pMovement.max_sneak_speed) applied_force = 1000 * pMovement.sneak_speed * Time.fixedDeltaTime * pMovement.movement_direction;
        else if (current_movement == Move_mode.running && rb.velocity.magnitude < pMovement.max_run_speed) applied_force = 1000 * pMovement.run_speed * Time.fixedDeltaTime * pMovement.movement_direction;
        else if ((current_movement == Move_mode.walking || current_movement == Move_mode.still)     && rb.velocity.magnitude < pMovement.max__speed) 
                applied_force = 1000 * pMovement.movement_speed * Time.fixedDeltaTime * pMovement.movement_direction;
        
        rb.AddForce(applied_force);

        //Jumping functionality
        if (Input.GetKeyDown(KeyCode.Space) && ground_sensor.detecting) { Debug.Log("jump"); rb.AddForce(Vector3.up * pMovement.jump_force); StartCoroutine(FallingInStyle()); }

        //refresh movement modes
        if (InputVector.magnitude < Mathf.Epsilon && ground_sensor.detecting) { current_movement = Move_mode.still; return; } //if grounded and not moving -still
        if (Input.GetKey(KeyCode.LeftShift)) { current_movement = Move_mode.running; return; }
        if (Input.GetKey(KeyCode.LeftControl)) { current_movement = Move_mode.sneaking; return; }
        current_movement = Move_mode.walking;
    }



    public IEnumerator FallingInStyle() //snappier falling
    {
        yield return new WaitForSeconds(0.15f);
        while (true)
        {
            yield return new WaitForFixedUpdate();
            if (rb.velocity.y < 0.0f) { rb.velocity += (pMovement.fall_multiplier - 1) * Physics.gravity.y * Time.fixedDeltaTime * Vector3.up; }
            if (ground_sensor.detecting) break;
        }
    }
    #endregion

    #region tracking and detection

    // tracks for smell, noise for hearing
    public IEnumerator LeaveBehindTracks() 
    {
        while (true) 
        {
            if (ground_sensor.detecting)
            {
                GameObject track = Instantiate(prefabTrack, ground_sensor.transform.position, Quaternion.identity);
                track.transform.SetParent(track_holder);
                yield return new WaitForSeconds(pDetection.intervalTrackSpawning);
            }
            else yield return new WaitForFixedUpdate();
        }
    }

    public void NoiseOfSteps() 
    {
        if (current_movement == Move_mode.sneaking) pDetection.stepInterval = pDetection.intAudioSneak;
        else if (current_movement == Move_mode.walking) pDetection.stepInterval = pDetection.intAudioWalk;
        else if (current_movement == Move_mode.running) pDetection.stepInterval = pDetection.intAudioRun;
        else return;    //don't make steps mid-ear doofus

        if (Time.time > pDetection.stepLastTime + pDetection.stepInterval)
        {
            pDetection.stepLastTime = Time.time;
            pDetection.StepTaken.Invoke(); 

            if (ground_sensor.detecting) 
            {
                Vector3 spawn_pos = ground_sensor.transform.position;

                //slight step offset
                pMovement.isLeftStepNow = !pMovement.isLeftStepNow;
                if (pMovement.isLeftStepNow) spawn_pos += head_child.right * pMovement.step_offset;

                GameObject track = Instantiate(prefabFootstep, spawn_pos, Quaternion.LookRotation(-transform.up, head_child.forward));
                track.transform.SetParent(track_holder);
            }
            
        }
    }


    public void UpdateDetectingEnemies() //update the list of curretly tracking and suspecting enemies
    {
        if(pDetection.current_detectors.Count>1) pDetection.current_detectors.Sort(SortByDetection);
        if (pDetection.current_detectors.Count > 0) pDetection.cur_Detection = pDetection.current_detectors[0].cur_detection;

        foreach (Detector d in enemy_holder.GetComponentsInChildren<Detector>())
        {
            if (d.detection_state == Detector.det_states.undetected) { if (pDetection.current_detectors.Contains(d)) pDetection.current_detectors.Remove(d); } //removing undetecting enemies 
            else
            {
                if (pDetection.current_detectors.Contains(d) == false) pDetection.current_detectors.Add(d); //Adding detecting and tracking enemies
                if (d.detection_state == Detector.det_states.suspected) UpdateDetectionWarning();
            }
        }
    }
    static int SortByDetection(Detector d1, Detector d2)    //in  a list of detectors, use this comparer to get top detection to list[0] lowest to list[n]
    {
        return d2.cur_detection.CompareTo(d1.cur_detection);
    }
    #endregion



    public void CheckIfFallen() //return to last checkpoint when the player falls out of the map
    {
        if (transform.position.y < -35) FallOffEdge();
    }
    public void FallOffEdge() 
    {
        transform.position = DataHolder.instance.cur_data.lastCheckpoint.transform.position;
    }


    private void UpdateDetectionWarning() 
    {
        //for each of the detectors in cur_detectors do

        //Determine relative direction
        //rotate the relative direction, from forward/backward to up/down
        //display detector direction
    }

    public void ShortcutDHTogglePause() => DataHolder.instance.TogglePause();
    public void ShortcutDHLoadMenu(int scene) => DataHolder.instance.SceneLoad(scene);
    public void ShortcutDHQuit() => DataHolder.instance.QuitGame();
    public void ShortcutUISettings() => UI_Handler.instance.ToggleSettings();
    

}
