using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
[ExecuteInEditMode]
public class Snapper : MonoBehaviour
{
    [System.Serializable]
    public class SnapSetting
    {
        public string path_of_effected = "World";
        public float precision_x = .5f;
        public float precision_y = .5f;
        public float precision_z = .5f;
        public Transform snap_centre;
    }

    [SerializeField] private SnapSetting[] Settings;
    //public List<SnapSetting> Settings;

    void Update()
    {
        if (EditorApplication.isPlaying) return;
        SnapSelectedObjects();
    }

    private void SnapSelectedObjects()
    {
        foreach (object selectedObject in Selection.objects)
        {
            GameObject selectedGameObject = selectedObject as GameObject;
            if (selectedGameObject == null) continue;

                SnapObjectToGrid(selectedGameObject);
            
        }
    }

    private void SnapObjectToGrid(GameObject gameObject)
    {
        SnapSetting settings = GetSnapping(gameObject);
        if (settings == null) return;

        float centerX = settings .snap_centre == null ? 0f : settings.snap_centre.position.x;
        float centerY = settings.snap_centre == null ? 0f : settings.snap_centre.position.y;
        float centerZ = settings.snap_centre == null ? 0f : settings.snap_centre.position.z;

        float deltaX = gameObject.transform.position.x - centerX;
        float deltaY = gameObject.transform.position.y - centerY;
        float deltaZ = gameObject.transform.position.z - centerZ;

        if (settings.precision_x >= Mathf.Epsilon)
        {
            float snappingX = 1 / settings.precision_x;
            deltaX = Mathf.Round(deltaX * snappingX) / snappingX;
        }
        if (settings.precision_y >= Mathf.Epsilon)
        {
            float snappingY = 1 / settings.precision_y;
            deltaY = Mathf.Round(deltaY * snappingY) / snappingY;
        }
        if (settings.precision_z >= Mathf.Epsilon)
        {
            float snappingZ = 1 / settings.precision_z;
            deltaZ = Mathf.Round(deltaZ * snappingZ) / snappingZ;
        }

        gameObject.transform.position = new Vector3(centerX + deltaX, centerY + deltaY, centerZ + deltaZ);
    }


    private SnapSetting GetSnapping(GameObject gameObject)
    {
        string hierarchyPath = GetGameObjectPath(gameObject.transform);
        foreach (SnapSetting snappingOverride in Settings)
        {
            if (hierarchyPath.Contains(snappingOverride.path_of_effected))
            {
                return snappingOverride;
            }
        }

        return null;
    }

    private static string GetGameObjectPath(Transform transform)
    {
        string path = transform.name;
        while (transform.parent != null)
        {
            transform = transform.parent;
            path = transform.name + "/" + path;
        }
        return path;
    }

}
#endif
