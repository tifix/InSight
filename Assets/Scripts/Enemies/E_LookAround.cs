using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[System.Serializable]
public class E_LookAround : E_Behaviour
{
   // public const behaviourType type = behaviourType.look;
   [Header("Looking specific params")]
    public float orientation_change_degree = 20;
    public float rotation_duration = 10;
    public AnimationCurve rotation_speed;

    private float yaw=0;
    private float pitch = 0;

    public IEnumerator LookAround() 
    {
        //Get current Yaw from rotation
        yaw = enemy_core.transform.eulerAngles.y;

        //initialize rotation slowly
        float init_yaw = yaw;
        float yaw_change = Random.Range(-orientation_change_degree, orientation_change_degree);

        rotation_duration = Mathf.Abs(yaw_change) / 40f;
        float elapsed = 0; float percent_elapsed = 0;

        //rotate over time
        while (percent_elapsed < 1)
        {
            elapsed += Time.deltaTime;
            percent_elapsed = elapsed / rotation_duration;
            yaw = Mathf.Lerp(init_yaw, init_yaw + yaw_change, rotation_speed.Evaluate(percent_elapsed));
            enemy_core.transform.eulerAngles = new Vector3(0, yaw, pitch);

            yield return new WaitForFixedUpdate();
        }
    }

    public override void LoopedAction() => enemy_core.StartCoroutine(LookAround());
}
