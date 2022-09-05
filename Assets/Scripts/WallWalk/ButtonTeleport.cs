
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class ButtonTeleport : UdonSharpBehaviour
{
    public Transform targetTransform;
    public Transform room;
    public Origin origin;

    public SphereRotationTest targetPlanet;
    public SphereRotationTest currentPlanet;

    public ParentHandler parentHandler;

    public override void Interact()
    {
        Networking.LocalPlayer.TeleportTo(targetTransform.position, targetTransform.rotation);
        origin._EnterRoom(room);
        if (Utilities.IsValid(currentPlanet))
        {
            currentPlanet.disableRotation();
        }
        if (Utilities.IsValid(targetPlanet))
        {
            targetPlanet.enableRotation();
        }
        parentHandler.SetParent(room);
    }
}
