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
        if(other.gameObject.CompareTag("Player"))
        Entered.Invoke();
    }

    protected void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player")) Left.Invoke();
    }
}
