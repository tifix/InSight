using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class DataHolder : MonoBehaviour
{
    //initialisation
    public enum area{ hub,hearing,smell,sight};
    public static DataHolder instance;

    //events
    public UnityEvent SceneLoaded;

    //subclasses
    [System.Serializable] public class Data 
    {
        public Transform lastCheckpoint;
        public float totalKills = 0;
        public float totalDetections = 0;
        public float totalObjectives = 0;
    }
    public Data cur_data = new();

    //variables
    public area curArea = area.hub;
    [SerializeField] private bool is_paused=false;




    
    

    private void Awake()
    {
        if (instance != null) Destroy(this);
        else instance = this;


        //Detecting each egg collected
        foreach (var item in GameObject.FindGameObjectsWithTag("Objective"))
        {
            item.GetComponent<EggObjective>().interaction.AddListener(AddObjective);
        }
    }

    #region GameData save interfacing

    public void AddObjective() 
    {
        cur_data.totalObjectives++;
    }
    public void AddDetection()
    {
        cur_data.totalDetections++;
    }

    public void AddKill()
    {
        cur_data.totalKills++;
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
        else if (curArea == sideB){ curArea = sideA; }

        Debug.LogWarning("now entering: " + curArea.ToString());
    }

    
    public void SetCheckpoint(Transform t) 
    {
        cur_data.lastCheckpoint = t;
    }
    
    public void SetRocksOpen(Animator anim) 
    {
        anim.SetTrigger("Open");
    }

    public void SetRocksClosed(Animator anim)
    {
        anim.SetTrigger("Closed");
    }
    public void SetRocksVanish(Animator anim)
    {
        anim.SetTrigger("Vanish");
    }


}
