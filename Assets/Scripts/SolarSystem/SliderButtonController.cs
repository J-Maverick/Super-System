using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using UnityEngine.UI;

public class SliderButtonController : UdonSharpBehaviour
{
    private Button button;
    public SliderController slider;

    void Start()
    {
        button = GetComponent<Button>();
    }

    public void DefaultSlider()
    {
        slider.ResetToDefault();
    }
    
    public void ZeroSlider()
    {
        slider.SetToZero();
    }

}
