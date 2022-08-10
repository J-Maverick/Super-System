
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class ButtonTeleport : UdonSharpBehaviour
{
    public Transform targetTransform;
    public Transform room;
    public Origin origin;

    public override void Interact()
    {
        Networking.LocalPlayer.TeleportTo(targetTransform.position, targetTransform.rotation);
        origin._EnterRoom(room);
    }
}
