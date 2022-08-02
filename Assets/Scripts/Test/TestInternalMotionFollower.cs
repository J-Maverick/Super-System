
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class TestInternalMotionFollower : UdonSharpBehaviour
{
    public testInternalMotion target;
    [UdonSynced] bool isGrabbed = false;

    void Update()
    {
        if (!isGrabbed)
        {
            transform.position = target.internalPosition;
        }
    }

    public override void OnPickup()
    {
        base.OnPickup();
        isGrabbed = true;
    }

    public override void OnDrop()
    {
        base.OnDrop();
        isGrabbed = false;
    }
}
