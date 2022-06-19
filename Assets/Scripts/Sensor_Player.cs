using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sensor_Player : Sensor
{
    public bool detectingWater;
    
    public override void OnTriggerStay(Collider other)
    {
        base.OnTriggerStay(other);
        if (other.CompareTag("Water")) detectingWater = true;
    }
    public override void OnTriggerExit(Collider other)
    {
        base.OnTriggerExit(other);
        detectingWater = false;
    }
}
