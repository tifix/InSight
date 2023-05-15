/*
 * 
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

public class UI_Handler : MonoBehaviour
{
    [SerializeField] GameObject UI_pause, UI_main,UI_cheats,UI_Victory; //Miscelanous game screens referenced

    //public Text counterDetection, counterKills, counterObjectives;
    public Text counterVictoryDetection, counterVictoryTime, counterVictoryObjectives;
    public Text textLoreScroll, captionLoreScroll;
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
    [Range(-3,3)]public float Brightness=1;
    public Volume caveVolume;
    public bool isShowingLoreScroll = false;


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

        if (Brightness != 1)
        {
            VolumeProfile profile = caveVolume.sharedProfile; 
            if (!profile.TryGet<Exposure>(out var exposure))
            {
                exposure = profile.Add<Exposure>(false);
            }
            exposure.compensation.value=1.5f + Brightness;
        }
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
        //counterDetection.text =GameManager.instance.cur_data.totalDetections.ToString();
        //counterKills.text =GameManager.instance.cur_data.totalDeaths.ToString();
        //counterObjectives.text =GameManager.instance.cur_data.totalObjectives.ToString();
    }

    public void ShowLoreNote(string noteTitle) //Display appropriate lore text. Logic invoked in GameManager.
    {
        isShowingLoreScroll = true;
        GetComponent<Animator>().SetTrigger("ScrollPickup");

        switch (noteTitle)
        {
            case "crystal1": 
                {
                    captionLoreScroll.text = "the Crystal Eggs 1/2";
                    textLoreScroll.text = 
                        "The misty ridge has long been avoided by travellers. " +
                        "Disruptive magical aura and tales of mighty beasts which recover " +
                        "from mightiest blows in an instant kept trade slow and the valley " +
                        "below quite peaceful. Some centuries ago a particularly stubborn " +
                        "researcher confirmed the creatures were indeed real. He reasoned " +
                        "that the creatures living at the ridge evolved to harness its " +
                        "energies - empowering themself as a result. He theorised that " +
                        "they pass their powers onto their stunning crystal eggs from which " +
                        "their young hatched.  If one was to claim one such egg they might " +
                        "be able to harness the power themself.";
                    break; 
                }
            case "crystal2":
                {
                    captionLoreScroll.text = "the Crystal Eggs 2/2";
                    textLoreScroll.text =
                        "The theory was ignored for decades, until a strange sickness befell " +
                        "the king’s only heir. No cure seemed to work. The most skilled " +
                        "healers and wizards - helpless. Desperate, the king ordered " +
                        "an expedition to the mountains. Two dozen greatest hunters set off " +
                        "for the ridge. Two men returned, clasping one crystal egg. The next " +
                        "morning, the Prince was cured. Celebrations followed and the egg was " +
                        "refined into a beautiful scarlet gemstone encased in the crown " +
                        "regalia. But the magic did not stop that day. The royal family " +
                        "seemed untouched by sickness. Years passed and stories began " +
                        "to circulate; that whoever possessed the gemstone would recover " +
                        "from any illness or damage overnight. The blessing of the artefact " +
                        "soon turned to a curse - a trail of blood followed it, as pretenders " +
                        "claimed the fabled Norhhern Star by blade and flame. At last, it " +
                        "rested in the hands and the Norhtern Star seemingly vanished from " +
                        "the world, never to be seen again.";
                    break;
                }
            case "intro":
                {
                    captionLoreScroll.text = "";
                    textLoreScroll.text =
                        "Two men climb up a steep path. Technically one man climbs, " +
                        "another rests on the first’s shoulders. The resting man is " +
                        "impossibly thin. Their young glimmering eyes seem ill fitted " +
                        "for this skeletal spectre. This is their reward for " +
                        "disobeying their family - within a week, this curse will " +
                        "turn them into a mindless zombie. The only cure you can " +
                        "turn to is the mythical crystal eggs of the beasts of misty " +
                        "ridge. Right, steal the crystal eggs, save your roommate " +
                        "from certain doom. Steal more and live happily ever after.";
                    break;
                }
            case "necrotic":
                {
                    captionLoreScroll.text = "Iris's Diary 05/05/712";
                    textLoreScroll.text =
                        "4 days ago He lost His sense of taste and smell. Since then, He lost " +
                        "a third of his weight. I was terrified. Then confused. Then I " +
                        "remembered my studies. Necrotic Decay. He will turn into a living " +
                        "corpse in 10 days. Whenever I found the curse was mentioned, there " +
                        "was one cure. Always one. Necrotic decay is material - bound. To halt " +
                        "the symptoms He must constantly touch the same gemstone that was " +
                        "used when the curse was cast. The only cure to stop this nightmare " +
                        "is encased in His fathe'r rapier. The northern star - The family " +
                        "heirloom. No one can steal it, let alone hold on to it so the curse " +
                        "does not resurface. He would rather die than return home anyways. " +
                        "And then it hit me - this gemstone was the only one in the hands of " +
                        "man, but not the only one in the world.";
                    break;
                }
            case "letter1":
                {
                    captionLoreScroll.text = "Letter to Mar (fragment)";
                    textLoreScroll.text =
                        "...I am glad, that despite this terrible " +
                        "distance my words can help You find the strength to keep " +
                        "going. To keep living and to keep trying to get out of the " +
                        "hell that is that future. Every night I wonder what the " +
                        "odds of us finding each other were. Every night I am in " +
                        "disbelief that you would want me, just as much as I crave " +
                        "you. It is not my place to ask how, or why... But I will " +
                        "act upon this fate. I will make the most... We will make " +
                        "the most of it. Soon. The day draws near. \n" +
                        "The anticipation is Killing me - signed, Iris";
                    break;
                }
            case "letter2":
                {
                    captionLoreScroll.text = "Letter to Mar (fragment)";
                    textLoreScroll.text =
                        "...Yet again they forced you to do it?  " +
                        "How can they keep getting away with it? No matter. " +
                        "All of that will not matter soon. I found myself " +
                        "daydreaming about You wondering through my orchard. " +
                        "It is so surreal that I will be able to kiss you after " +
                        "what felt like eternity. I will kiss you by the apples. " +
                        "I will kiss you by the pond, I will kiss You awake in the " +
                        "morning with soft sunlight on your soft lips. I will make up " +
                        "for the time we have lost to them by loving you thrice as " +
                        "much as anyone would think possible. \n" +
                        "- signed, Iris";
                    break;
                }
            case "letter3":
                {
                    captionLoreScroll.text = "Letter to Mar (fragment)";
                    textLoreScroll.text =
                        "...Oh my, I could not see myself woodworking. " +
                        "These arms are not made for chopping trees. " +
                        "Planting them is much more to my liking. " +
                        "But the image of you swinging it is... " +
                        "a pleasant one. Mayhaps if you hurt Yourself " +
                        "out there I could tend to You..?  All that " +
                        "strength and I would still have you helpless. " +
                        "All for your health, naturally. - signed, Iris \n" +
                        "P.S. Did I mention I make tremendous healing stew?";
                    break;
                }
            case "letter4":
                {
                    captionLoreScroll.text = "Letter to Iris (fragment)";
                    textLoreScroll.text =
                        "...Patience, patience... \n" +
                        "This is such a struggle; I know wonderful times " +
                        "are just around the corner. I know I will " +
                        "have you all for myself so... so soon, but even " +
                        "though it is such a wonderful prospect, the " +
                        "wait is utterly unbearable. With every day" +
                        "I feel like you invade my mind even more. " +
                        "Frankly I feel like I am going mad. \n" +
                        "P.S. I need my cure. I need you. \n" +
                        "-signed Mar";
                    break;
                }
            case "beasts":
                {
                    captionLoreScroll.text = "Research Diary 08/10/325";
                    textLoreScroll.text =
                        "... though the low level of light in the cavern " +
                        "made the creatures practically blind, their hearing " +
                        "became all the sharper. I made it this far by using " +
                        "the loud waterfalls to conceal my footsteps, but I am " +
                        "unsure whether that will remain a possibility further in. " +
                        "Caution is advised; I stepped into a puddle once and the" +
                        "noice echoed across half the cave. My blood nearly froze." +
                        "I have caught glimpses of their nests, but to gather any " +
                        "samples would require me to pass right by to the creatures...";
                    break;
                }
            default:
                {
                    captionLoreScroll.text = "Soggy Paper";
                    textLoreScroll.text = "*you can't make out the writing as the ink got warped by water*";
                    break;
                }
                
        }


    }
    public void HideLoreNote()
    {
        isShowingLoreScroll = false;
        GetComponent<Animator>().SetTrigger("ScrollClose");
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
    public void SetBrightness(float _bright)
    {
        Brightness = _bright;
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
