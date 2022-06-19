using System.Collections;
using System.Collections.Generic;
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
