
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class SkyboxRotate : UdonSharpBehaviour
{
    public Transform sun;
    public bool rotationActive = true;

    public void Update()
    {
        if (rotationActive)
        {
            transform.rotation = sun.rotation;
        }
    }

    public void ToggleRotation()
    {
        rotationActive = !rotationActive;
    }
}
