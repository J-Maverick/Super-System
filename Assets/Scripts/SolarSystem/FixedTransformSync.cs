
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class FixedTransformSync : UdonSharpBehaviour
{
    [UdonSynced] public Vector3 position = Vector3.zero;
    //[UdonSynced(UdonSyncMode.Smooth)] public Vector3 velocity = Vector3.zero;
    [UdonSynced] public Quaternion rotation = Quaternion.identity;

    private VRCPlayerApi owningPlayer = null;

    private Vector3 previousSyncedPosition = Vector3.zero;
    private Vector3 recentSyncedPosition = Vector3.zero;
    private Vector3 velocity = Vector3.zero;

    float previousTime = 0f;
    float timeSinceLastSync = 0f;

    private const int frameUpdateCount = 1;
    private const float emaWeight = 0.5f;

    public void SetPositionRotation(Vector3 newPos, Quaternion newRot)
    {
        position = newPos;
        rotation = newRot;
        //velocity = newVel;
        //RequestSerialization();
    }

    public void Start()
    {
        owningPlayer = Networking.GetOwner(gameObject);
        previousTime = Time.realtimeSinceStartup;
    }

    public override void OnOwnershipTransferred(VRCPlayerApi player)
    {
        owningPlayer = player;
    }

    public void SetOwningPlayer(VRCPlayerApi player)
    {
        owningPlayer = player;
    }

    public override void OnDeserialization()
    {
        previousSyncedPosition = recentSyncedPosition;
        recentSyncedPosition = position;

        timeSinceLastSync = Time.realtimeSinceStartup - previousTime;
        previousTime = Time.realtimeSinceStartup;
        if (timeSinceLastSync < 0.001f) timeSinceLastSync = 0.001f;

        velocity = emaWeight * ((recentSyncedPosition - previousSyncedPosition) / timeSinceLastSync) + (1f - emaWeight) * velocity;
    }

    //private Quaternion DeltaRotation(Vector3 eulerAngle, float deltaTime)
    //{
    //    Vector3 halfAngle = eulerAngle * deltaTime * 0.5f;
    //    float angleMagnitude = halfAngle.magnitude;

    //    if (angleMagnitude > 0f)
    //    {
    //        halfAngle *= Mathf.Sin(angleMagnitude) / angleMagnitude;
    //        return new Quaternion(Mathf.Cos(angleMagnitude), halfAngle.x, halfAngle.y, halfAngle.z);
    //    }
    //    else return new Quaternion(1.0f, halfAngle.x, halfAngle.y, halfAngle.z);
    //}

    private void FixedUpdate()
    {
        if (Time.frameCount % frameUpdateCount == 0) RequestSerialization();
    }

    private void Update()
    {
        if (owningPlayer != null && !owningPlayer.isLocal)
        {
            //Debug.LogFormat("{0}: Integrating position for player {1}[{2}]", name, owningPlayer.displayName, owningPlayer.playerId);
            position += velocity * Time.deltaTime;
        }
    }
}
