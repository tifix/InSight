using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DevTools : MonoBehaviour
{
    public bool devToolsEnabled  = false;
    public List<Transform> checkpoints;
    public static DevTools instance;

    public void Awake()
    {
        if (instance == null) instance = this; else Destroy(this);
    }
    public void ToggleDetections(bool targetState)
    {
        if (!devToolsEnabled) return;
        if (targetState) Player.instance.pDetection.mulGlobal = 1;
        else Player.instance.pDetection.mulGlobal = 0;
    }

    //[SerializeField]
    public void WarpPlayer(Transform destination)
    {
        if (!devToolsEnabled) return;
        Player.instance.transform.position = destination.transform.position;
    }
    public void WarpViaScrollList(Int32 value) 
    {
        WarpPlayer(checkpoints[value]);   
    }


    [SerializeField]
    private void ToggleAllRocks(bool targetState)
    {
        int num = 0;
        if (!devToolsEnabled) return ;

        foreach (GameObject rocks in GameObject.FindGameObjectsWithTag("FallingRocks"))
        {
            num++;
            if(targetState==false) rocks.GetComponent<Animator>().SetTrigger("Vanish");
            else rocks.GetComponent<Animator>().SetTrigger("Closed");
        }
        Debug.Log("rocks affected: " + num);
        return;
    }
}
