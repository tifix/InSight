using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
[RequireComponent(typeof(SphereCollider))]

public class SmellNode : KillAfterTime
{

    public float perc_passed =0;
    public AnimationCurve range_dropoff;

    public override void Start()
    {
        base.Start();
        name = SetName();

        spawn_time = Time.time;
        CheckIfOnWater();
        StartCoroutine(UpdateScent(CheckIfOnWater()));
    
    
    }

    string SetName() 
    {
        float t = Mathf.Round(Time.time * 20);
        t /= 20;
        return ("Track " + t.ToString() + "s ");
    }

    bool CheckIfOnWater() 
    {
        Collider[] cols=Physics.OverlapSphere(transform.position, 0.2f);
        foreach (Collider col in cols) 
        {
            if (col.transform.gameObject.CompareTag("Water")) { Debug.LogWarning("In Water!"); return true; }
        }

        return false;
    }

    // Update is called once per frame
    IEnumerator UpdateScent(bool on_water)
    {
        if (on_water) lifetime /= 2;

        while (perc_passed<1) 
        {
            perc_passed = (Time.time - spawn_time) / lifetime;
            transform.localScale=Vector3.one*range_dropoff.Evaluate(perc_passed);
            yield return new WaitForSeconds(0.25f);
            
        }
    }
}
