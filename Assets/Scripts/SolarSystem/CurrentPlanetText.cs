
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using UnityEngine.UI;

public class CurrentPlanetText : UdonSharpBehaviour
{
    public Origin origin;
    public Text text;

    public void Update()
    {
        if (origin.currentRoom != null) text.text = string.Format("Current: {0}", origin.currentRoom.name);
    }
}
