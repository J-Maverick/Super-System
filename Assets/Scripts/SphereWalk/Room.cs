
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class Room : UdonSharpBehaviour
{
    //[UdonSynced]
    public int roomID;
    public Origin origin;

    private void Start()
    {
        origin.AddRoom(this);
    }
}
