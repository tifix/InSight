using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Detector))]

public class EnemyCore : MonoBehaviour
{
    public Detector detector;
    //public E_Behaviour.behaviourType[] behaviour_types;
    public E_Behaviour[] Behaviours;    // = new E_Behaviour[3]
    
    public enemy_state cur_state=enemy_state.idle;

    public enum enemy_state {idle,tracking,in_combat };


    private void Awake()
    {
        /*
        for (int i = 0; i < behaviour_types.Length; i++)
        {
            if (behaviour_types[i] == E_Behaviour.behaviourType.walk) Behaviours[i] = new E_WalkToPlayer();
            else if (behaviour_types[i] == E_Behaviour.behaviourType.look) Behaviours[i] = new E_LookAround();
        }
        */

        detector = GetComponent<Detector>();
    }

    // Start is called before the first frame update
    void Start()
    {

        detector.pl_detected.AddListener(SwitchToCombat);
        detector.pl_start_tracking.AddListener(SwitchToTracking);
        detector.pl_end_tracking.AddListener(SwitchToIdle);

    }

    void SwitchState(enemy_state target_state) 
    {
        cur_state = target_state;

        foreach(E_Behaviour behaviour in Behaviours) 
        {
            //Disabling expired behaviours
            if (behaviour.parent_state != cur_state &&behaviour.is_performing) behaviour.TogglePerforming();

            //Starting newly appropriate behaviours
            if (behaviour.parent_state == cur_state && !behaviour.is_performing) behaviour.TogglePerforming();
        }
    }
    
    public void SwitchToCombat() 
    {
        SwitchState(enemy_state.in_combat);
    }
    public void SwitchToTracking()
    {
        SwitchState(enemy_state.tracking);
    }
    public void SwitchToIdle()
    {
        SwitchState(enemy_state.idle);
    }
}
