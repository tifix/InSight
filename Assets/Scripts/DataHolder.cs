using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class DataHolder : MonoBehaviour
{
    //initialisation
    public enum area { hub, hearing, smell, sight };
    public static DataHolder instance;
    public Light SunLight;

    //events
    public UnityEvent SceneLoaded;

    //subclasses
    [System.Serializable] public class Data
    {
        public Transform lastCheckpoint;
        public int totalDeaths = 0;
        public int totalDetections = 0;
        public int totalObjectives = 0;
        [Tooltip("should show as 0 until Victory Screen is shown")] public float totalGameTime=0;
    }
    public Data cur_data = new();

    public DevTools cheats;
    public int neededObjetives;

    //variables
    public area curArea = area.hub;
    [SerializeField] private bool is_paused = false;




    private void Awake()
    {
        if (instance != null) Destroy(this);
        else instance = this;


        //Detecting each egg collected
        /*
        foreach (var item in GameObject.FindGameObjectsWithTag("Objective"))
        {
            item.GetComponent<EggObjective>().interaction.AddListener(AddObjective);
        }
        */
    }

    #region GameData save interfacing

    public void AddObjective()
    {
        Debug.Log("objective++");
        cur_data.totalObjectives++;
    }
    public void AddDetection()
    {
        Debug.Log("detection++");
        cur_data.totalDetections++;
    }

    public void AddDeath()
    {
        cur_data.totalDeaths++;
    }

    #endregion

    //Invoked by player controller
    public void TogglePause()
    {
        //toggling time distortion
        is_paused = !is_paused;
        if (is_paused) Time.timeScale = 0.002f;
        else Time.timeScale = 1;

        //toggling cursor unlock
        if (Cursor.lockState == CursorLockMode.None) { Cursor.lockState = CursorLockMode.Locked; Cursor.visible = false; }
        else { Cursor.lockState = CursorLockMode.None; Cursor.visible = true; }

        UI_Handler.instance.TogglePauseUI();
    }
    public void QuitGame() => Application.Quit();


    public void SceneLoad(int scene)
    {
        Time.timeScale = 1;
        StartCoroutine(IE_SceneLoading(scene));
    }
    public IEnumerator IE_SceneLoading(int scene)
    {
        SceneManager.LoadScene(scene);
        yield return new WaitForSeconds(1);
        SceneLoaded.Invoke();
    }



    public void SetArea(area sideA, area sideB) //Ensure the area edges are super slim, so that it's impossible to not walk through them in their entirety instantly.
    {
        if (curArea == sideA) { curArea = sideB; }
        else if (curArea == sideB) { curArea = sideA; }

        Debug.LogWarning("now entering: " + curArea.ToString());
    }


    public void SetCheckpoint(Transform t) => cur_data.lastCheckpoint = t;


    [SerializeField] private void SetRocksOpen(Animator anim) => anim.SetTrigger("Open");
    public void SetRocksClosed(Animator anim) => anim.SetTrigger("Closed");
    public void SetRocksVanish(Animator anim) => anim.SetTrigger("Vanish");


    public void WarpViaScrollList(Int32 value)
    { Debug.Log("warping to position #" +value); DevTools.instance.WarpPlayer(DevTools.instance.checkpoints[value]); }


    public void TriggerVictory()
    {
        cur_data.totalGameTime = Time.timeSinceLevelLoad;
        UI_Handler.instance.ShowVictoryScreen();
    }


    public void SpawnAtPosition(GameObject what, Transform where) 
    {
        GameObject GO =GameObject.Instantiate(what, where);
        GO.transform.SetParent(null);
    }

    public void SpawnVFXCheckpoint(Transform t) =>SpawnAtPosition(Resources.Load("VFX/VFX_Interacted_pinch") as GameObject, t);
    public void SpawnVFXObjective(Transform t) =>SpawnAtPosition(Resources.Load("VFX/VFX_Interacted_cyllinder") as GameObject, t);
    
    public void SetSunlight(bool state) 
    {
        if(SunLight.gameObject.activeInHierarchy!= state) SunLight.gameObject.SetActive(state);
    }

}
