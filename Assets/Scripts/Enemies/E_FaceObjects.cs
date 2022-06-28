using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class E_FaceObjects : E_Behaviour
{
    public float rotDuration = 0.25f;
    public GameObject target;
    public bool autoTargetAudio = false;

    public override void LoopedAction() 
    {
        if (autoTargetAudio) enemy_core.StartCoroutine(LookAtObject(enemy_core.gameObject.GetComponent<Det_Audio>().RefreshAudioEntities()));
        else enemy_core.StartCoroutine(LookAtObject(Player.instance.gameObject));
    }
    public IEnumerator LookAtObject(GameObject GO)
    {
        Quaternion q_init;
        Quaternion q_target;
        try
        {
            q_init = Quaternion.LookRotation(transform.forward);
            q_target = Quaternion.LookRotation(GO.transform.position - transform.position);
        }
        catch { Debug.Log("Player instance not yet defined"); yield break; }

        yield return new WaitForFixedUpdate();



        float elapsed = 0, percent_elapsed = 0;

        //rotate over time
        while (percent_elapsed < 1)
        {
            elapsed += Time.deltaTime;
            percent_elapsed = elapsed / rotDuration;
            Quaternion q_cur = Quaternion.Lerp(q_init, q_target, percent_elapsed);
            enemy_core.transform.rotation = q_cur;

            yield return new WaitForFixedUpdate();
        }

    }
}
