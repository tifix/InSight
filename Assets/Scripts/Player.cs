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
    public bool isGrounded = false;
    public bool isMovementLocked = false;
    public bool isInSafeSpace = false;
    [System.Serializable] private class PlrSpeed
    {
        //Movement backend values
        public Vector3  movement_direction, dir_memory;
        public Vector3  surfaceNormal;
        public float    slope_limit =   30;
        public float meshRotationStrength = 0.05f;
        //public LayerMask viableTerrain;
        [Tooltip("from transform.position to ground"),Range(0,1f)] public float groundDistance = 1.5f;
        [Space(2)]
        //Speed balancing values
        public float    tapMaxDuration = 0.5f, tapInitialisedTimestamp = 0f, tapForce=20;
        public float    movementForce= 60,  sneakForce = 30,   runForce = 100;
        public float    max_sneak_speed = 3, max_run_speed = 7, max__speed = 5;

        public float    fall_multiplier=3, fall_uptime=0.15f;       //artificial snappy gravity
        public float    jump_force=     20750;
        [Space(2)]
        //Step VFX related
        public bool         isLeftStepNow = false;
        public Transform    stepPositionL, stepPositionR;
        public AudioClip[]  stepClips;
        public AudioClip[]  stepWetClips;
    }
    [Tooltip("Speed parameters")] [SerializeField] private PlrSpeed pMovement;
    [System.Serializable] public class PlrDetections
    {
                            public float cur_Detection;
                            public bool isDetectionGainFrozen;
                            public bool isInSafeSpace;
                            public List<Detector> current_detectors = new();

        [Header("current detection multipliers")]
        [Range(0, 4)]       public float mulGlobal = 1;
        [Range(0, 4)]       public float mulVisualCur = 1, mulAudioCur = 1, mulSmellCur = 1;                            //current detection multipliers
        [Range(0, 4)]       public float mulVisualRun = 2f, mulVisualSneak = 0.3f;                                      //when running, detect faster, when sneaking - slower
        [Range(0, 4)]       public float mulAudioStill = 0.2f, mulAudioSneak = 1f, mulAudioWalk = 2f, mulAudioRun = 3.5f, mulAudioWater=1.5f;                  //when running, detect faster, when sneaking- slower, when still- just barely
        [Range(0, 4)]       public float intAudioSneak = 0.75f, intAudioWalk = 0.5455f, intAudioRun = 0.4286f;          //8-BPM  110BPM 140BPM
        [Range(0, 4)]       public float intervalTrackSpawning=1f;
        [Range(0, 4)]       public float stepInterval = 0.05f, stepLastTime = 0;
        [HideInInspector]   public UnityEvent StepTaken=new UnityEvent();
    }
    [Space(2)]
    [ Tooltip("detection parameters")] public PlrDetections pDetection;

    public enum Move_mode { still, sneaking, walking, running, inAir}

    [Header("Related objects")]

                        public  Sensor_Player       ground_sensor;
                 static public  Player              instance;
                        public  CameraController    cam_cont;
    [HideInInspector]   public  Rigidbody           rb;

    [SerializeField]    private GameObject          prefabTrack;
    [SerializeField]    private GameObject          prefabFootstep;
    [SerializeField]    private GameObject          prefabWaterSplash, prefabWaterJump;
                        public  AudioSource         stepsSource;
                        public  Transform           enemy_holder;
                        public  Transform           track_holder;
                        public  Transform           last_checkpoint;
    [SerializeField]    private Transform           mesh_child;
    [SerializeField]    private Transform           head_child;

                        public  ParticleSystem      VFX_DetectFlash;
    [SerializeField]    private ParticleSystem      VFX_DeathFlash;
    [SerializeField]    private Animator            animator;
    [SerializeField]    private float               lockMovementInteractionLength=1.25f;

    #region monobehaviour integrations


    void Start()=> StartCoroutine(ContinousTrackSpawning());
    

    void Update()
    {
        //show the highest detection on detect slider
        UI_Handler.instance.detection_a.value = pDetection.cur_Detection;
        UI_Handler.instance.detection_b.value = pDetection.cur_Detection;

        //Mesh lil movements
        mesh_child.rotation = Quaternion.LookRotation(mesh_child.forward + VectorFlattenY(pMovement.movement_direction * pMovement.meshRotationStrength),Vector3.up);   //gently rotate the player in the direction moved
        Quaternion q1 = Quaternion.LookRotation(head_child.forward + cam_cont.transform.right * 0.09f, Vector3.up);                                                     //rotate the head in direction looked
        head_child.rotation = q1;

        //tap tap movement gtting startInput time
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.S) 
         || Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.D)) StartCoroutine(FreezeDetctionWhenTapping());

        //when key released super fast, do a tiny impulse movement
        if (pDetection.isDetectionGainFrozen)
        if(Input.GetKeyUp(KeyCode.W) || Input.GetKeyUp(KeyCode.S)
        || Input.GetKeyUp(KeyCode.A) || Input.GetKeyUp(KeyCode.D)) LilDash();  //;
    }



    private void FixedUpdate()
    {
        isGrounded = UpdateIfGrounded(); //updating whether player is grounded
        if(!isMovementLocked) MovementInput();
        ApplyMoveModes();
        ApplyStepBehaviours();

        UpdateDetectingEnemies();

        CheckIfFallen();
    }

    #endregion

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
                    if (isInWater) pDetection.mulAudioCur *= pDetection.mulAudioWater;
                    pDetection.intervalTrackSpawning = 1/ pDetection.mulGlobal;
                break;
            }
        case Move_mode.walking:
            {
                pDetection.mulVisualCur = 1 * pDetection.mulGlobal;    
                pDetection.mulAudioCur = pDetection.mulAudioWalk * pDetection.mulGlobal;
                    if(isInWater) pDetection.mulAudioCur *= pDetection.mulAudioWater;
                pDetection.intervalTrackSpawning = 0.5f / pDetection.mulGlobal;
                break;
            }
        case Move_mode.running:
            {
                pDetection.mulVisualCur = pDetection.mulVisualRun * pDetection.mulGlobal;
                pDetection.mulAudioCur = pDetection.mulAudioRun * pDetection.mulGlobal;
                    if (isInWater) pDetection.mulAudioCur *= pDetection.mulAudioWater;
                pDetection.intervalTrackSpawning = 0.25f / pDetection.mulGlobal;
                break;
            }
        case Move_mode.inAir:
            {
                pDetection.mulVisualCur = pDetection.mulVisualRun * pDetection.mulGlobal;
                pDetection.mulAudioCur = pDetection.mulAudioRun * pDetection.mulGlobal;
                if (isInWater) pDetection.mulAudioCur *= pDetection.mulAudioWater;
                pDetection.intervalTrackSpawning = 99 / pDetection.mulGlobal;
                break;
            }
            default:
            {
                //passively return to  walking multiplier
                pDetection.mulVisualCur = 1 * pDetection.mulGlobal;
                pDetection.mulAudioCur = pDetection.mulAudioWalk * pDetection.mulGlobal;
                pDetection.intervalTrackSpawning = 0.5f / pDetection.mulGlobal;
                break;
            }   
        }    
    }

    IEnumerator FreezeDetctionWhenTapping()
    {
        yield return new WaitForEndOfFrame();
        pDetection.isDetectionGainFrozen = true;
        yield return new WaitForSeconds(pDetection.stepInterval-Mathf.Epsilon);     //remove invulnerability when 2nd step is about to be taken
        pDetection.isDetectionGainFrozen = false;
    }

    void LilDash()  => rb.AddForce(pMovement.dir_memory * pMovement.tapForce, ForceMode.Impulse);
    

    public Vector3 GetSurfaceNormal() 
    {
        Ray r = new(mesh_child.position+ mesh_child.up*0.1f, -transform.up);  // providing tiny margin to ensure raycast doesn't clip through the floor
        //Debug.DrawRay(transform.position + transform.up * 2, Vector3.up* .3f, Color.green);

        if (Physics.Raycast(r, out RaycastHit hit, 250,LayerMask.GetMask("Ground"))) 
        {
            Debug.DrawRay(transform.position + transform.up * 2, hit.normal * .3f, Color.green);
            if(Vector3.Angle(hit.normal, Vector3.up)>pMovement.slope_limit) return Vector3.up;

            return hit.normal;
        }
        return Vector3.up;
    }
    Vector3 GetSurfaceAdjustedForward(Vector2 InputVector)
    {
        pMovement.surfaceNormal = GetSurfaceNormal();

        Vector3 MoveForward = Vector3.Cross(pMovement.surfaceNormal, cam_cont.transform.forward * InputVector.y).normalized;
        Vector3 MoveSideways = Vector3.Cross(pMovement.surfaceNormal, cam_cont.transform.right * InputVector.x).normalized;

        Debug.DrawRay(transform.position + transform.up * 2, MoveForward * .3f, Color.blue);
        Debug.DrawRay(transform.position + transform.up * 2, MoveSideways * .3f, Color.red);
        Debug.DrawRay(transform.position + transform.up * 2, cam_cont.transform.right * .3f, Color.yellow);
        
        return MoveForward + MoveSideways;
    }
    public Vector3 VectorFlattenY(Vector3 inVect3) 
    {
        return new Vector3(inVect3.x, 0, inVect3.z);
    }

    void MovementInput()
    {
        Vector2 InputVector = new Vector3(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        pMovement.movement_direction = GetSurfaceAdjustedForward(InputVector);
        if (pMovement.movement_direction != Vector3.zero) pMovement.dir_memory = pMovement.movement_direction;



        //applying the force depending on the current movement mode and maximum speed values
        Vector3 applied_force = Vector3.zero;
        if (current_movement == Move_mode.sneaking && rb.velocity.magnitude < pMovement.max_sneak_speed) applied_force = 1000 * pMovement.sneakForce * Time.fixedDeltaTime * pMovement.movement_direction;
        else if (current_movement == Move_mode.running && rb.velocity.magnitude < pMovement.max_run_speed) applied_force = 1000 * pMovement.runForce * Time.fixedDeltaTime * pMovement.movement_direction;
        else if ((current_movement == Move_mode.walking || current_movement == Move_mode.still) && rb.velocity.magnitude < pMovement.max__speed)
            applied_force = 1000 * pMovement.movementForce * Time.fixedDeltaTime * pMovement.movement_direction;

        rb.AddForce(applied_force);


        animator.SetBool("inAir", !ground_sensor.detecting);

        //Jumping functionality
        if (Input.GetKeyDown(KeyCode.Space) && ground_sensor.detecting)
        {
            current_movement = Move_mode.inAir;
            Debug.Log("jump");
            rb.AddForce(Vector3.up * pMovement.jump_force);
            animator.SetTrigger("Jump");

            StartCoroutine(ContinousFalling());
        }
        //animator.transform.rotation

        //Updating animations according to movement inputs
        animator.SetFloat("movementY", Mathf.Abs(InputVector.normalized.magnitude));
        //animator.SetFloat("movementY", InputVector.y);



        //refresh movement modes
        if (InputVector.magnitude < Mathf.Epsilon && isGrounded && current_movement!=Move_mode.inAir) { current_movement = Move_mode.still; return; } //if grounded and not moving -still
        if (Input.GetKey(KeyCode.LeftShift))    { current_movement = Move_mode.running;                             return; }
        if (Input.GetKey(KeyCode.LeftControl))  { current_movement = Move_mode.sneaking;                            return; }
        if (!ground_sensor.detecting)           { current_movement = Move_mode.inAir;                               return; }
        current_movement = Move_mode.walking;
    }

    bool UpdateIfGrounded() //Raycast do not work on non-convex mesh colliders, ergo detector trigger is needed. 
    {        
        isInWater = ground_sensor.detectingWater;
        return ground_sensor.detecting;
    }


    public IEnumerator ContinousFalling() //snappier falling
    {
        current_movement = Move_mode.inAir;
        yield return new WaitForSeconds(pMovement.fall_uptime);
        while (true)
        {
            Debug.DrawRay(transform.position, Vector3.down,Color.red);
            yield return new WaitForFixedUpdate();
            //if (rb.velocity.y < 0.0f) { 
                rb.AddForce(Physics.gravity * Time.fixedDeltaTime * pMovement.fall_multiplier, ForceMode.Acceleration); //}
            if (isGrounded) break;
        }
        Debug.LogWarning("Player landed!");
        pDetection.StepTaken.Invoke();
        if (isInWater) 
        { 
            GameObject bigSploosh = Instantiate(prefabWaterJump, ground_sensor.transform.position, Quaternion.LookRotation(transform.up, transform.forward));
            stepsSource.PlayOneShot(pMovement.stepWetClips[0]);
        }
        else stepsSource.PlayOneShot(pMovement.stepClips[0]);

        yield return new WaitForSeconds(0.1f);
        current_movement = Move_mode.still;
    }
    #endregion

    public void SetAnimationTrigger(string s) { animator.SetTrigger(s); if (s=="Using") Player.instance.LockMovementForTime(lockMovementInteractionLength); }

    #region tracking and detection

    // tracks for smell, noise for hearing
    public IEnumerator ContinousTrackSpawning() 
    {
        while (true) 
        {
            if (isGrounded)
            {
                GameObject track = Instantiate(prefabTrack, transform.position+Vector3.down* pMovement.groundDistance, Quaternion.identity);
                track.transform.SetParent(track_holder);
                yield return new WaitForSeconds(pDetection.intervalTrackSpawning);
            }
            else yield return new WaitForFixedUpdate();
        }
    }
    public void LockMovementForTime(float time) => StartCoroutine(MovementLock(time));
    IEnumerator MovementLock(float time) { isMovementLocked = true;yield return new WaitForSecondsRealtime(time); isMovementLocked = false; }


    public void ApplyStepBehaviours() 
    {
        if (current_movement == Move_mode.sneaking) pDetection.stepInterval = pDetection.intAudioSneak;
        else if (current_movement == Move_mode.walking) pDetection.stepInterval = pDetection.intAudioWalk;
        else if (current_movement == Move_mode.running) pDetection.stepInterval = pDetection.intAudioRun;
        else return;    //don't make steps mid-ear doofus

        if (Time.time > pDetection.stepLastTime + pDetection.stepInterval)
        {
            if (isGrounded) 
            {
                if (!isInWater) stepsSource.PlayOneShot(pMovement.stepClips[Random.Range(0, pMovement.stepClips.Length - 1)]);  //plays a random step sound or a wet sploosh when in water.
                else stepsSource.PlayOneShot(pMovement.stepWetClips[Random.Range(0, pMovement.stepWetClips.Length - 1)]);

                SpawnFootmarks(); //spawning footstep marks and water splashes
            }

            pDetection.stepLastTime = Time.time;        //updating times
            pDetection.StepTaken.Invoke();

        }
    }
    private void SpawnFootmarks()
    {
        Vector3 spawn_pos = ground_sensor.transform.position;
        
        //spawn water splashes
        if (isInWater) 
        {
            GameObject waterSplash = Instantiate(prefabWaterSplash, spawn_pos, Quaternion.LookRotation(transform.up, transform.forward));
        }

        //slight step offset
        pMovement.isLeftStepNow = !pMovement.isLeftStepNow;
        if (pMovement.isLeftStepNow) spawn_pos = pMovement.stepPositionL.position; else spawn_pos = pMovement.stepPositionR.position;

        GameObject track = Instantiate(prefabFootstep, spawn_pos, Quaternion.LookRotation(-transform.up, head_child.forward));
        track.transform.SetParent(track_holder);
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
    public void FallOffEdge() => Die(" falling off the edge");

    public void SetIsInSafespace(bool state) { instance.pDetection.isInSafeSpace = state; }

    //
    public void Die(string context) 
    {
        VFX_DeathFlash.Play();
        transform.position = GameManager.instance.cur_data.lastCheckpoint.transform.position;
        Debug.LogWarning("You died from: "+context);
        GameManager.instance.AddDeath();

        //Clearing detection values on death
        FindObjectOfType<EnemyCore>().EndEngaging();
        isMovementLocked = false;
    }
    //
    //
    //

    public void ShortcutDHTogglePause() => GameManager.instance.TogglePause();
    public void ShortcutDHLoadMenu(int scene) => GameManager.instance.SceneLoad(scene);
    public void ShortcutDHQuit() => GameManager.instance.QuitGame();
    public void ShortcutUISettings() => UI_Handler.instance.ToggleSettings();
    

}
