
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class SphereRotationTest : UdonSharpBehaviour
{
    private VRCPlayerApi localPlayer;
    private Vector3 lastPosition;
    private bool rotationActive = false;

    private Vector3 startPosition;
    private Quaternion startRotation;

    public Origin origin;

    void Start()
    {
        localPlayer = Networking.LocalPlayer;
        lastPosition = localPlayer.GetPosition();

        startPosition = transform.position;
        startRotation = transform.rotation;
    }

    public override void OnPlayerRespawn(VRCPlayerApi player)
    {
        if (rotationActive)
        {
            disableRotation();
            player.Respawn();
        }
    }

    public void enableRotation()
    {
        rotationActive = true;
        lastPosition = localPlayer.GetPosition();
    }

    public void disableRotation()
    {
        transform.SetPositionAndRotation(startPosition, startRotation);
        rotationActive = false;
    }

    private void Update()
    {
        Vector3 playerPos = localPlayer.GetPosition();
        if (rotationActive)
        {
            Vector3 movement = playerPos - lastPosition;
            if (movement.magnitude < 0.001f) return;
            float angle = movement.magnitude * (180f / Mathf.PI) / (playerPos - transform.position).magnitude;

            Vector3 rotationAxis = Vector3.Cross(Vector3.up, movement).normalized;
            transform.localRotation = Quaternion.Euler(rotationAxis * -angle) * transform.localRotation;

            lastPosition = playerPos;

            playerPos.y = transform.position.y;
            transform.position = playerPos;
        }
        else lastPosition = playerPos;
    }
}
