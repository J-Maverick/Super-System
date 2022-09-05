
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using UnityEngine.UI;

public class TeleportButton : UdonSharpBehaviour
{
    private Button button;
    public PlanetWalkingHandler planetWalkingHandler;
    public PlanetFollower PlanetFollowerTarget;

    void Start()
    {
        button = GetComponent<Button>();
    }
    
    public void SetPlanetWalkTarget()
    {
        planetWalkingHandler.SetPlanetWalkTarget(PlanetFollowerTarget);
    }   
}
