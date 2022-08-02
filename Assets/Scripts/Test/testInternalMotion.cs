
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class testInternalMotion : UdonSharpBehaviour
{
    [UdonSynced(UdonSyncMode.Linear)] public Vector3 internalPosition = Vector3.zero;
    public Vector3 internalVelocity = Vector3.zero;

    void Start()
    {
        
    }

    void FixedUpdate()
    {
        if (Networking.LocalPlayer.isMaster)
        {
            internalVelocity = Vector3.one * Mathf.Cos(Time.fixedTime);
            internalPosition += internalVelocity * Time.fixedDeltaTime;
        }
    }
}
