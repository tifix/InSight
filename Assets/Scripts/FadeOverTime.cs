using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

[RequireComponent(typeof(DecalProjector))]
public class FadeOverTime : KillAfterTime
{
    float perc_passed=0;
    DecalProjector decal;

    // Start is called before the first frame update
    public override void Start()
    {
        decal = GetComponent<DecalProjector>();
        base.Start();
        StartCoroutine(Fade());
    }
    private IEnumerator Fade() 
    {
        while (perc_passed < 1) 
        {
            perc_passed = (Time.time - spawn_time) / lifetime;
            decal.fadeFactor = 1-perc_passed;
            yield return new WaitForSeconds(0.5f);
        }
    }
}
