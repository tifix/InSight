using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable] public class XYEvent : UnityEvent<DataHolder.area, DataHolder.area> { }
//[System.Serializable] public class YZEvent : UnityEvent<float, float> { }

public class AreaEdge : MonoBehaviour
{
    public DataHolder.area a1, a2;
    
    public XYEvent areaSwitch;

    protected void OnTriggerEnter(Collider other)
    {
        areaSwitch.Invoke(a1, a2);
    }
}
