/*
 * 
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class UI_Handler : MonoBehaviour
{
    [SerializeField] GameObject UI_pause, UI_main,UI_cheats,UI_Victory; //Miscelanous game screens referenced

    public Text counterDetection, counterKills, counterObjectives;
    public Text counterVictoryDetection, counterVictoryTime, counterVictoryObjectives;
    public Slider detection_a, detection_b, volume;
    [SerializeField] GameObject directionArrow, DirectionalGizmo;             //direction of spotting enemy arrow
    [SerializeField] AudioMixer audioMixer;
    public static UI_Handler instance;
    public Animator locationOverlay;
    [Header("Detection bar colours")]
    [SerializeField] Color clr_suspected = Color.HSVToRGB(21f/ 360, 0.85f, 0.71f);//new Color(0.183f, 0.84f,0.27f);
    [SerializeField] Color clr_detected = Color.HSVToRGB(334/ 360f, 1f, 0.71f);
    [SerializeField] Color clr_tracked = Color.HSVToRGB(260f/ 360, 0.87f, 0.72f);
    [SerializeField] Transform DetectingEnemy;
    public GameObject sunReal, sunCave;

    void Start()
    {
        if (UI_pause.activeInHierarchy) UI_pause.SetActive(false);//GameManager.instance.TogglePause();

        SetDetectionColorSus();
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }
    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Backspace)) GameManager.instance.TogglePause();
        if (GameManager.instance.cheats.timeScale!=1) Time.timeScale=GameManager.instance.cheats.timeScale;
        if (Player.instance.pDetection.current_detectors.Count > 0 && !directionArrow.activeInHierarchy)
        {
            DirectionalGizmo.GetComponent<Animator>().SetTrigger("GizmoShow");
            DetectingEnemy = Player.instance.pDetection.current_detectors[0].transform; 
        }
        
        if (Player.instance.pDetection.current_detectors.Count > 0) { SetDirectionDetection(DetectingEnemy.position); }
        else DirectionalGizmo.GetComponent<Animator>().SetTrigger("GizmoHide");
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
        ToggleCheatsDisplay(GameManager.instance.cheats.devToolsEnabled);
        counterDetection.text =GameManager.instance.cur_data.totalDetections.ToString();
        counterKills.text =GameManager.instance.cur_data.totalDeaths.ToString();
        counterObjectives.text =GameManager.instance.cur_data.totalObjectives.ToString();
    }


    public void SetDirectionDetection(Vector3 target) 
    {
        Vector3 relativeVect = target - Player.instance.transform.position;
        relativeVect = new Vector3(relativeVect.x, 0, relativeVect.z);
        Debug.DrawRay(Player.instance.transform.position + Vector3.up * 2, relativeVect);
        DirectionalGizmo.transform.rotation = Quaternion.LookRotation(transform.up, relativeVect);   //LookAt(target, Vector3.up);
    }

    
    //main functionality in GameManager, invoked by player controller
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
    }

    public void SetVolume(float volume) 
    {
        audioMixer.SetFloat("volumeMaster", volume);
    }
    public void ShowVictoryScreen() 
    {
        UI_Victory.SetActive(true);
        counterVictoryObjectives.text = GameManager.instance.cur_data.totalObjectives.ToString();
        counterVictoryDetection.text = GameManager.instance.cur_data.totalDetections.ToString();
        counterVictoryTime.text = GameManager.instance.cur_data.totalGameTime.ToString();
    }


    public static void SetAnimatorTrigger(string _name) 
    {
        instance.locationOverlay.SetTrigger(_name);
    }
}
