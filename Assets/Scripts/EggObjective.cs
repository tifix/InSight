using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EggObjective : Interactible
{
    public GameObject eggs;

    public override void Interaction()
    {
        base.Interaction();
        eggs.SetActive(false);
    }
}
