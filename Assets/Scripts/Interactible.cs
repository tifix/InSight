using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider))]
public class Interactible : MonoBehaviour
{
    public bool         inRange     = false;
    public bool         isAutomatic     = false;
    public bool         wasUsed     = false;
    public bool         singleUse   = false;
    public UnityEvent   interaction;


    void OnTriggerEnter(Collider other) => inRange = true;
    void OnTriggerExit(Collider other) => inRange = false;


    void FixedUpdate()
    {
        if (
            (Input.GetKeyDown(KeyCode.E) && inRange) || (isAutomatic && inRange)
            ) Interaction();
    }

    public virtual void Interaction() 
    {
        wasUsed = true;
        interaction.Invoke();
        if (singleUse) Destroy(this);
    }
}
