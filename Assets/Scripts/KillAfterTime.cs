using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillAfterTime : MonoBehaviour
{
    public float spawn_time;
    public float lifetime = 30;

    public virtual void Start() { spawn_time = Time.time; Destroy(gameObject, lifetime); }
}
