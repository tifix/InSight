/*
 * Trigger when entering and or leaving.
*/
using UnityEngine;
using UnityEngine.Events;

public class InvokeOnTrigger : MonoBehaviour
{
    [SerializeField] private LayerMask viableUsers;
    [SerializeField] private bool isSingleUse;
    [SerializeField] private bool destroyOnEnter;
    [SerializeField] private bool destroyOnExit;
    [SerializeField] private UnityEvent Entered=null;
    [SerializeField] private UnityEvent Left=null;

    protected virtual void OnTriggerEnter(Collider other)
    {
        if ((viableUsers & 1 << other.gameObject.layer) == 1 << other.gameObject.layer)
        {
            Entered.Invoke();
            if (isSingleUse && destroyOnEnter)  Destroy(this);
        }
    }

    protected void OnTriggerExit(Collider other)
    {
        if ((viableUsers & 1 << other.gameObject.layer) == 1 << other.gameObject.layer)
        {
            Left.Invoke();
            if (isSingleUse && destroyOnExit) Destroy(this);
        }
    }
}
