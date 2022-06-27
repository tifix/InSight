using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class UI_Handler : MonoBehaviour
{
    public GameObject UI_pause, UI_main,UI_cheats,UI_Victory; //UI_settings

    public Text counterDetection, counterKills, counterObjectives;
    public Text counterVictoryDetection, counterVictoryTime, counterVictoryObjectives;
    public Slider detection_a, detection_b, volume;
    public AudioMixer audioMixer;
    public static UI_Handler instance;

    public Color clr_suspected = Color.HSVToRGB(21f/ 360, 0.85f, 0.71f);//new Color(0.183f, 0.84f,0.27f);
    public Color clr_detected = Color.HSVToRGB(334/ 360f, 1f, 0.71f);
    public Color clr_tracked = Color.HSVToRGB(260f/ 360, 0.87f, 0.72f);

    void Start()
    {
        if (UI_pause.activeInHierarchy) UI_pause.SetActive(false);//DataHolder.instance.TogglePause();

        SetDetectionColorSus();
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }
    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Backspace)) DataHolder.instance.TogglePause();
    }


    public void SetDetectionColorSus() 
    { 
        detection_a.fillRect.GetComponent<Image>().color = clr_suspected; 
        detection_b.fillRect.GetComponent<Image>().color = clr_suspected; 
    }
    public void SetDetectionColorDet()
    {
        detection_a.fillRect.GetComponent<Image>().color = clr_detected;
        detection_b.fillRect.GetComponent<Image>().color = clr_detected;
    }
    public void SetDetectionColorTrack()
    {
        detection_a.fillRect.GetComponent<Image>().color = clr_tracked;
        detection_b.fillRect.GetComponent<Image>().color = clr_tracked;
    }



    public void RefreshDisplays() 
    {
        ToggleCheatsDisplay(DevTools.instance.devToolsEnabled);
        counterDetection.text =DataHolder.instance.cur_data.totalDetections.ToString();
        counterKills.text =DataHolder.instance.cur_data.totalKills.ToString();
        counterObjectives.text =DataHolder.instance.cur_data.totalObjectives.ToString();
    }


    
    //main functionality in DataHolder, invoked by player controller
    public void TogglePauseUI() 
    {
        RefreshDisplays();
        UI_main.SetActive(!UI_main.activeInHierarchy);
        UI_pause.SetActive(!UI_pause.activeInHierarchy);
    }
    public void ToggleCheatsDisplay(bool tarState) => UI_cheats.SetActive(tarState);
    

    public void ToggleSettings()
    {
        UI_pause.SetActive(!UI_pause.activeInHierarchy);
        //UI_settings.SetActive(!UI_settings.activeInHierarchy);
    }

    public void SetVolume(float volume) 
    {
        audioMixer.SetFloat("volumeMaster", volume);
    }
    public void ShowVictoryScreen() 
    {
        UI_Victory.SetActive(true);
        counterVictoryObjectives.text = DataHolder.instance.cur_data.totalObjectives.ToString();
        counterVictoryDetection.text = DataHolder.instance.cur_data.totalDetections.ToString();
        counterVictoryTime.text = DataHolder.instance.cur_data.totalGameTime.ToString();
    }

}
