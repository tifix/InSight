using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(EnemyCore))]
//[System.Serializable]
public class E_Behaviour : MonoBehaviour
{
    //public enum behaviourType {walk,look,patrol,D,E };
    [Header("general behaviour parameters")]
    public bool is_performing = false;
    public EnemyCore.enemy_state parent_state;
    public float base_interval = 2;
    public float interval_randomness = 1;
    
    protected EnemyCore enemy_core;

    
    public virtual void Start()
    {
        enemy_core = GetComponent<EnemyCore>();
        ApplyPerforming();
    }
    

    public virtual void TogglePerforming()
    {
        is_performing = !is_performing;
        ApplyPerforming();
    }

    public virtual void TogglePerforming(bool isPerforming)
    {
        is_performing = isPerforming;
        ApplyPerforming();
    }

    public virtual IEnumerator BehaviourLoop() 
    {
        while (is_performing)
        {
            //perform the action
            LoopedAction();
            //wait for next rotation
            yield return new WaitForSeconds(base_interval + Random.Range(-interval_randomness * base_interval, interval_randomness * base_interval));
        }
    }
    public void ApplyPerforming() 
    {
        if (is_performing) enemy_core.StartCoroutine(BehaviourLoop());
        else enemy_core.StopCoroutine(BehaviourLoop());
    }
    
    public virtual void LoopedAction() 
    {
    
    }

}
