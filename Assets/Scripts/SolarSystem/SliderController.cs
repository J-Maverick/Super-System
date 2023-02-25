using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using UnityEngine.UI;

public class SliderController : UdonSharpBehaviour
{
    public Slider slider;
    [UdonSynced] private float value;
    public Text text;
    public SimulationSpace simulationSpaceTarget;
    public float sliderMultiplier = 1000f;

    public string sliderText = "x";
    private bool deserializing;

    private VRCPlayerApi localPlayer;
    private float defaultValue;


    void Start()
    {
        deserializing = false;
        if (slider != null)
        {
            defaultValue = slider.value;
        }
    }

    public override void OnDeserialization()
    {
        deserializing = true;
        if (!Networking.IsOwner(gameObject) && slider != null)
        {
            slider.value = value;
        }
        deserializing = false;
    }

    public override void OnPreSerialization()
    {
        if (slider != null)
        {
            value = slider.value;
        }
    }

    public override void OnPlayerJoined(VRCPlayerApi player)
    {
        RequestSerialization();
    }

    public void UpdateSimulationTimeStep()
    {
        // Get ownership of this object and the object you want the slider to affect
        GetOwnership(gameObject);
        GetOwnership(simulationSpaceTarget.gameObject);

        // Do whatever you want to do with the object that you are affecting with slider
        simulationSpaceTarget.timeStep = slider.value;

        // Sync the networked value
        RequestSerialization();
    }

    public void UpdateGravityMultiplier()
    {
        GetOwnership(gameObject);
        GetOwnership(simulationSpaceTarget.gameObject);
        simulationSpaceTarget.gravitationMultiplier = slider.value;
        RequestSerialization();
    }

    public void ResetToDefault()
    {
        slider.value = defaultValue;
    }

    public void SetToZero()
    {
        slider.value = 0f;
    }

    private void GetOwnership(GameObject objectToOwn)
    {
        simulationSpaceTarget.Sync();
        if (!Networking.IsOwner(objectToOwn) && !deserializing)
        {
            Networking.SetOwner(localPlayer, objectToOwn);
        }
    }
    
    private void Update()
    {
        if (slider != null)
        {
            UpdateSliderText();
        }
    }

    public void UpdateSliderText()
    {
        text.text = string.Format("{0:F1}{1}", slider.value * sliderMultiplier, sliderText);
    }
}
