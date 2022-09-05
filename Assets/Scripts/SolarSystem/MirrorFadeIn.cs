
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class MirrorFadeIn : UdonSharpBehaviour
{
    private VRCPlayerApi localPlayer;
    private MeshRenderer mesh;
    private float alpha = 0f;

    public float maxDistance = 20f;
    public float minDistance = 10f;

    private float distance;

    [Range(0f, 1f)]
    public float minAlpha = 0f;
    [Range(0f, 1f)]
    public float maxAlpha = 1f;

    void Start()
    {
        localPlayer = Networking.LocalPlayer;
        mesh = GetComponent<MeshRenderer>();
    }

    void UpdateAlpha()
    {
        Color color = mesh.material.color;
        color.a = alpha;
        mesh.material.color = color;
    }
    
    public override void OnPlayerTriggerEnter(VRCPlayerApi player)
    {
        if (player.isLocal)
        {
            alpha = minAlpha;
            UpdateAlpha();
            mesh.enabled = true;
        }
    }

    public override void OnPlayerTriggerExit(VRCPlayerApi player)
    {
        if (player.isLocal)
        {
            alpha = minAlpha;
            UpdateAlpha();
            mesh.enabled = false;
        }
    }

    public override void OnPlayerTriggerStay(VRCPlayerApi player)
    {
        if (player.isLocal)
        {
            distance = (localPlayer.GetPosition() - transform.position).magnitude;
            if (distance > maxDistance) alpha = minAlpha;
            else if (distance < minDistance) alpha = maxAlpha;
            else if (distance >= minDistance && distance <= maxDistance) alpha = maxAlpha - ((maxAlpha - minAlpha) * ((distance - minDistance) / (maxDistance - minDistance)));

            UpdateAlpha();
        }
    }
}
