/*
 * Returns true continously while something in it's range. False, otherwise.
*/
using UnityEngine;

public class Sensor : MonoBehaviour
{
    public bool detecting;
    
    //NOT INHERITED BY SMELL
    public virtual void OnTriggerStay(Collider other)
    {
     detecting = true;
    }
    public virtual void OnTriggerExit(Collider other)
    {
        detecting = false;
    }
}
