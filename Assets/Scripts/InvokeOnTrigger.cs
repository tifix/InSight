using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class InvokeOnTrigger : MonoBehaviour
{

    public UnityEvent Entered=null;
    public UnityEvent Left=null;

    protected virtual void OnTriggerEnter(Collider other)
    {
        Entered.Invoke();
    }

    protected void OnTriggerExit(Collider other)
    {
        Left.Invoke();
    }
}
