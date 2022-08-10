using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using UnityEngine.UI;

public class ButtonController : UdonSharpBehaviour
{
    private Button button;
    public SimulationSpace target;
    Vector3 targetInitialPosition;
    Quaternion targetInitialRotation;

    void Start()
    {
        button = GetComponent<Button>();
        targetInitialPosition = target.transform.position;
        targetInitialRotation = target.transform.rotation;
    }

    public void ResetSolarSystem()
    {
        Networking.SetOwner(Networking.LocalPlayer, target.gameObject);
        target.ResetPlanets();
    }

    public void ToggleTrails()
    {
        target.ToggleTrails();
    }
    
    public void SyncSolarSystem()
    {
        target.Sync();
    }

    public void ToggleAutoSync()
    {
        target.SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "ToggleAutoSync");
    }
}
