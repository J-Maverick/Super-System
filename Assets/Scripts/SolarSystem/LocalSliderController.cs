
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using UnityEngine.UI;

public class LocalSliderController : UdonSharpBehaviour
{
    private Slider slider;
    public Text text;
    public Animator animatorTarget;
    public float sliderMultiplier = 1000f;
    public SimulationSpace simulationSpaceTarget;

    public string sliderText = "x";

    void Start()
    {
        slider = GetComponent<Slider>();
    }

    public void UpdateSunOpacity()
    {
        animatorTarget.SetFloat("Opacity", slider.value);
    }

    private void Update()
    {
        UpdateSliderText();
    }

    public void UpdateSliderText()
    {
        text.text = string.Format("{0:F1}{1}", slider.value * sliderMultiplier, sliderText);
    }

    public void UpdateSimFidelity()
    {
        simulationSpaceTarget.forceUpdateDelay = sliderMultiplier - (sliderMultiplier * slider.value);
    }
}
