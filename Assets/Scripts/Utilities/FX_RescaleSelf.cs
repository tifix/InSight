/*
 * Scales the GameObject Transform scale or RectTransform scale over time
 * 
 */
using UnityEngine;
using UnityEngine.UI;

public class FX_RescaleSelf : MonoBehaviour
{
    [SerializeField] float init_scale = 0.1f;
    [SerializeField] float pulseOffset = 0.1f;
    [SerializeField] float pulseAmplitude = 0.1f;
    [SerializeField] float pulseFrequency = 1;
    [SerializeField] Color color;

    public void Update()
    {
        if(TryGetComponent<RectTransform>(out RectTransform rt)) 
        { rt.localScale = (Mathf.Abs(Mathf.Sin(pulseOffset+Time.time * pulseFrequency)) * pulseAmplitude + init_scale) * Vector3.one; }
        else { transform.localScale = (Mathf.Abs(Mathf.Sin(pulseOffset+Time.time * pulseFrequency)) * pulseAmplitude + init_scale) * Vector3.one; }
        
        if (TryGetComponent<Text>(out Text txt)) txt.color = color;
    }
}
