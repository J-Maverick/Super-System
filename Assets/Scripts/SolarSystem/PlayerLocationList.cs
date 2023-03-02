
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using UnityEngine.UI;

public class PlayerLocationList : UdonSharpBehaviour
{
    public StationController[] stations;
    public Text text;
    public string playerList = "";
    private float nextTime = 0f;
    private float intervalTime = 0.5f;

    private void Start()
    {
        UpdateText();
    }

    private void Update()
    {
        _UpdateAtInterval();
    }

    private void _UpdateAtInterval()
    {
        if (Time.realtimeSinceStartup > nextTime)
        {
            UpdateText();
            nextTime = Time.realtimeSinceStartup + intervalTime;
        }
    }
    
    private void UpdateText()
    {
        playerList = "";
        foreach (StationController station in stations)
        {
            VRCPlayerApi stationOwner = Networking.GetOwner(station.gameObject);
            if (station.gameObject.activeSelf && stationOwner.IsValid() && station.currentRoom != null)
            {
                if (stationOwner.isMaster) playerList += string.Format("(Master) {0}: {1}\n", stationOwner.displayName, station.currentRoom.name);
                else playerList += string.Format("{0}: {1}\n", stationOwner.displayName, station.currentRoom.name);
            }
        }
        text.text = playerList;
    }
}
