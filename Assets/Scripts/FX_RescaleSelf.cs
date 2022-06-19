using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FX_RescaleSelf : MonoBehaviour
{
    public float init_scale = 0.1f;

    public float pulseOffset = 0.1f;
    public float pulseAmplitude = 0.1f;
    public float pulseFrequency = 1;
    public Color color;

    // Update is called once per frame
    public void Update()
    {
        if(TryGetComponent<RectTransform>(out RectTransform rt)) 
        { rt.localScale = (Mathf.Abs(Mathf.Sin(pulseOffset+Time.time * pulseFrequency)) * pulseAmplitude + init_scale) * Vector3.one; }
        else { transform.localScale = (Mathf.Abs(Mathf.Sin(pulseOffset+Time.time * pulseFrequency)) * pulseAmplitude + init_scale) * Vector3.one; }
        
        if (TryGetComponent<Text>(out Text txt)) txt.color = color;
    }
}
