
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class testPickup : UdonSharpBehaviour
{
    public GameObject target;
    [UdonSynced] bool isGrabbed = false;

    void Start()
    {
        transform.position = target.transform.position;
    }

    private void Update()
    {
        if (!isGrabbed)
        {
            transform.position = target.transform.position;
        }
    }
    void OnPickup()
    {
        isGrabbed = true;
    }

    void OnDrop()
    {
        isGrabbed = false;
    }
}
