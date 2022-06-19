using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider))]
public class Interactible : MonoBehaviour
{
    public bool inRange = false;
    public bool wasUsed = false;
    public UnityEvent interaction;


    private void OnTriggerEnter(Collider other) => inRange = true;
    private void OnTriggerExit(Collider other) => inRange = false;

    // Update is called once per frame
    void FixedUpdate()
    {
        if (Input.GetKeyDown(KeyCode.E) && inRange) Interaction();
    }

    public virtual void Interaction() 
    {
        Debug.Log("Here");
        wasUsed = true;
        interaction.Invoke();
    }
}
