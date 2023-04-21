using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable, ExecuteInEditMode]
public class DevTools
{
    public bool devToolsEnabled  = false;
    public List<Transform> checkpoints;
    public Transform warpHere = null;
    [Range(0, 2f)] public float timeScale = 1;
    [Tooltip("Hide all enemies for simpler exploration")] public bool isEnemiesDisabled = false;

    public void ToggleDetections(bool targetState)
    {
        if (!devToolsEnabled) return;
        if (targetState) Player.instance.pDetection.mulGlobal = 1;
        else Player.instance.pDetection.mulGlobal = 0;
    }

    public void WarpViaScrollList(Int32 value) 
    {
        GameManager.instance.WarpSmart(checkpoints[value]);   
    }

    public void RemoveEnemies() 
    {
        try
        {
            GameObject.FindObjectOfType<EnemyCore>().gameObject.transform.parent.gameObject.SetActive(false);
        }
        catch { }
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
