
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class FixedTransformSync : UdonSharpBehaviour
{
    public Vector3 position = Vector3.zero;
    public Quaternion rotation = Quaternion.identity;

    private VRCPlayerApi owningPlayer = null;

    [UdonSynced] public Vector3 targetPosition = Vector3.zero;
    [UdonSynced] public Quaternion targetRotation = Quaternion.identity;

    private Vector3 velocity = Vector3.zero;
    private Vector3 angularVelocity = Vector3.zero;

    float previousTime = 0f;
    float timeSinceLastSync = 0f;

    private const int frameUpdateCount = 5;
    private const float emaWeight = 1f;

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

    public override void OnPreSerialization()
    {
        if (owningPlayer != null && owningPlayer.isLocal)
        {
            targetRotation = rotation;
            targetPosition = position;
        }
    }

    public override void OnDeserialization()
    {
        timeSinceLastSync = Time.realtimeSinceStartup - previousTime;
        previousTime = Time.realtimeSinceStartup;
        if (timeSinceLastSync < 0.001f) timeSinceLastSync = 0.001f;

        //velocity = emaWeight * ((recentSyncedPosition - previousSyncedPosition) / timeSinceLastSync) + (1f - emaWeight) * velocity;
        velocity = (targetPosition - position) / timeSinceLastSync;
        angularVelocity = (targetRotation.eulerAngles- rotation.eulerAngles) / timeSinceLastSync;
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
        if (owningPlayer != null && !owningPlayer.isLocal)
        {
            //Debug.LogFormat("{0}: Integrating position for player {1}[{2}]", name, owningPlayer.displayName, owningPlayer.playerId);
            position = Vector3.MoveTowards(position, targetPosition, velocity.magnitude * Time.fixedDeltaTime);
            rotation = Quaternion.RotateTowards(rotation, targetRotation, angularVelocity.magnitude * Time.fixedDeltaTime);
        }
    }

    private void Update()
    {
        if (Time.frameCount % frameUpdateCount == 0) RequestSerialization();
    }
}
