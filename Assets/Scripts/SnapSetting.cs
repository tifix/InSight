using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SnapSetting
{
    public string path_of_effected="World";
    public float precision_x=.5f;
    public float precision_y=.5f;
    public float precision_z=.5f;
    public Transform snap_centre;
}
