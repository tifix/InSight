/*
 * Used for detecting specifically smell nodes
 */
using System.Collections.Generic;
using UnityEngine;

public class SensorFiltering : Sensor 
{
    public List<SmellNode> allDetected = new List<SmellNode>();

    public void FixedUpdate()
    {
        if (allDetected.Count > 0) detecting = true;
        else detecting = false;
    }

    //PURGE OG OTS
    public override void OnTriggerStay(Collider other)
    {
    }

    public void OnTriggerEnter(Collider other)
    {
        //if triggering object has the desired component
        if (other.gameObject.TryGetComponent<SmellNode>(out SmellNode node)) allDetected.Add(node);
    }

    public override void OnTriggerExit(Collider other)
    {
        //base.OnTriggerExit(other);

        SmellNode seeked_component = other.gameObject.GetComponent<SmellNode>();

        //if triggering object has the desired component
        if (seeked_component != null)
        {
            allDetected.Remove(seeked_component);
            allDetected.TrimExcess();
        }
    }

}



