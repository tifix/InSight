using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class InvokeStay : InvokeOnTrigger
{
    public UnityEvent Stay = null;
    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
            Stay.Invoke();
    }
}
