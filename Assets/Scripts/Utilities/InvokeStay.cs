/*
 * Invoke SOMETHING every frame when within the collider. Includes entry and exit too.
*/
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider))]
public class InvokeStay : InvokeOnTrigger
{
    public UnityEvent Stay = null;
    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
            Stay.Invoke();
    }
}
