using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weather : MonoBehaviour
{
    public float wind_change_interval = 10;
    public static Vector3 wind;
    public float min_windspeed=0.5f;
    public float max_windspeed=3;

    // Start is called before the first frame update
    void Start()
    {
        Weather.wind = GenerateNewWind();
        StartCoroutine(WindUpdating());
    }

    private IEnumerator WindUpdating() 
    {
        while (true) 
        {
            Weather.wind = GenerateNewWind();
            ApplyWind();
            yield return new WaitForSeconds(wind_change_interval);
        }
    }

    public Vector3 GenerateNewWind() 
    {
        Vector3 vect=Vector3.right*Random.Range(-3f,3f);
        vect += Vector3.forward * Random.Range(-3f, 3f);
        vect.Normalize();
        vect *= Random.Range(min_windspeed, max_windspeed);
        return vect;
    
    }

    public void ApplyWind() 
    {
        /*
        foreach(WindZone wz in GetComponentsInChildren<WindZone>()) 
            {
            wz.gameObject.transform.rotation=Quaternion.LookRotation(wind);
            }
        */

        foreach(Cloth cl in GetComponentsInChildren<Cloth>()) 
        {
            cl.externalAcceleration = Weather.wind;
        }
    }

}
