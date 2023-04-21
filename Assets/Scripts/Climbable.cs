/*
 * Ropes, ladders - going up or down. 
 * Can be enabled or enabled from above, or enabled by default.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Climbable : MonoBehaviour
{
    public Transform top, bottom;
    Vector3 curPos = Vector3.zero;
    public float climbTime = 2;
    public bool isUsable;
    public Animator anim;
    [SerializeField] Interactible unlocking;

    public void SetUsable(bool state) { isUsable = state; }
    public void Unlock() { SetUsable(true); anim.SetTrigger("Unlock"); }
    void Start()
    {
        if (anim == null) anim = GetComponent<Animator>();
    }

    public void StartClimb()
    {

        if (Vector3.Distance(bottom.position, Player.instance.transform.position)
            < Vector3.Distance(top.position, Player.instance.transform.position)) //if closer to the bottom than top, head up
        {
            StartCoroutine(ContinousClimbing(true));
        }
        else StartCoroutine(ContinousClimbing(false));
    }

    private IEnumerator ContinousClimbing(bool isHeadingUp)
    {
        Debug.Log("Climbing! Heading up? " + isHeadingUp.ToString());
        float compltion = 0;

        //Snap rotation and position to the Climbable rock
        if (isHeadingUp) { curPos = bottom.position; Player.instance.cam_cont.transform.rotation = Quaternion.LookRotation(bottom.forward, Vector3.up); }
        else { curPos = top.position; Player.instance.cam_cont.transform.rotation = Quaternion.LookRotation(top.forward, Vector3.up); }

        while (compltion < 1)
        {
            if (isHeadingUp) curPos = Vector3.Lerp(bottom.position, top.position, compltion);
            else curPos = Vector3.Lerp(top.position, bottom.position, compltion);

            Player.instance.transform.position = curPos;

            compltion += Time.fixedDeltaTime / climbTime;    //total duration of Lerp is 1s, total duration is trippled, by slowing incrementation 3-fold.
            yield return new WaitForFixedUpdate();
        }

        if (isHeadingUp) //small boost forward when leaving at the top
        {
            compltion = 0;
            Vector3 finPos = top.position + top.forward * 1.75f;
            while (compltion < 1)
            {
                curPos = Vector3.Lerp(top.position, finPos, compltion);
                Player.instance.transform.position = curPos;

                compltion += Time.fixedDeltaTime / 0.5f;
                yield return new WaitForFixedUpdate();
            }

        }

    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawCube(curPos, Vector3.one * 0.5f);
        Gizmos.DrawCube(top.position, Vector3.one * 0.3f);
        Gizmos.DrawCube(bottom.position, Vector3.one * 0.3f);
    }
}
