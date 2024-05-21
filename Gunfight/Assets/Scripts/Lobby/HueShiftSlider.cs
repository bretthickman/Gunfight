using UnityEngine;
using UnityEngine.UI;

public class HueShiftSlider : MonoBehaviour
{
    public Slider hueSlider;

    public Material targetMaterial;
    private float hueShiftValue = 0f;

    void Start()
    {
        UpdateHueShift();
    }

    public void OnSliderValueChanged()
    {
        hueShiftValue = hueSlider.value;
        Debug.Log(hueShiftValue + " HUESHIFT");
        UpdateHueShift();
    }

    void UpdateHueShift()
    {
        // Set the hue shift parameter in the shader
        targetMaterial.SetFloat("_HsvShift", hueShiftValue);
    }
}
