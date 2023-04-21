using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

//[System.Serializable] public class XYEvent : UnityEvent<GameManager.area, GameManager.area> { }

[ExecuteInEditMode]
public class AreaEdge : MonoBehaviour
{
    public GameManager.area aBlue, aRed;
    public UnityEvent triggered;
    public LayerMask viableUser;
//    public XYEvent areaSwitch;

    public void Awake()
    {
        //triggered.AddListener(EnterEvent);
    }

    protected void OnTriggerEnter(Collider other) 
    {
        Debug.Log("Enter trigger");

        if ((viableUser & 1 << other.gameObject.layer) != 1 << other.gameObject.layer) return;

        if (GameManager.instance.curArea == aBlue)
        {
            GameManager.instance.curArea = aRed;
            Debug.Log("Now entering: " + aRed.ToString());
            triggered.Invoke(); return;
        }
        else if (GameManager.instance.curArea == aRed)
        {
            GameManager.instance.curArea = aBlue;
            Debug.Log("Now entering: " + aBlue.ToString());
            triggered.Invoke(); return;
        }
        else { Debug.LogWarning("Area edge state did not shift properly A. Check naming?"); }
        
    } 
    


    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(transform.position + transform.forward * .4f,0.2f);
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(transform.position + transform.forward * -.4f, 0.2f);
    }

    public void EnterEvent() 
    {
        Debug.Log("Enter event");

        //triggering overlay animation based on what area was entered
        switch (GameManager.instance.curArea) 
        {
            case (GameManager.area.hub): { UI_Handler.SetAnimatorTrigger("CampEntered"); UI_Handler.instance.sunReal.SetActive(true); UI_Handler.instance.sunCave.SetActive(false); break; }
            case (GameManager.area.grotto): { UI_Handler.SetAnimatorTrigger("GrottoEntered"); UI_Handler.instance.sunReal.SetActive(false); UI_Handler.instance.sunCave.SetActive(true); break; }
            case (GameManager.area.canyon): { UI_Handler.SetAnimatorTrigger("CanyonEntered"); break; }
            case (GameManager.area.peaks): { UI_Handler.SetAnimatorTrigger("PeaksEntered"); break; }
            
            default: { Debug.LogWarning("Area edge state did not shift properly B. Check naming?"); break; }
        }
       
    }

}
