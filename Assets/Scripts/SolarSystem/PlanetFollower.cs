using UdonSharp;
using VRC.SDKBase;
using VRC.Udon;
using UnityEngine;
using VRC.Udon.Common;

public class PlanetFollower : UdonSharpBehaviour
{
    #region Class Properties
    public PlanetWalkingHandler planetWalkingHandler;
    
    public bool planetIsKill = false;
    public bool planetCollided = false;
    
    public GravitationalObject followerTarget = null;

    private bool warmUp = true;
    private float warmUpTime = .5f;
    private float warmUpTimer = 0f;

    private VRCPlayerApi localPlayer;
    public bool planetWalking = false;
    private Vector3 lastPosition;
    private Vector3 planetWalkPosition;
    private Quaternion planetWalkRotation;

    public bool autoSync = false;
    
    public Matrix4x4 preTransformWorldToLocalMatrix;

    private bool firstRespawn = false;

    #endregion

    #region Unity Callbacks


    void LateUpdate()
    {
        if (!warmUp)
        {
            if (followerTarget == null)
            {
                Networking.Destroy(gameObject);
                Destroy(gameObject);
            }
            else
            {
                transform.localPosition = followerTarget.position;
                transform.localRotation = followerTarget.transform.localRotation;
                transform.localScale = followerTarget.transform.localScale;

                planetIsKill = followerTarget.planetIsKill;
                planetCollided = followerTarget.planetCollided;

                if (!Utilities.IsValid(localPlayer)) localPlayer = Networking.LocalPlayer; 

                if (Utilities.IsValid(localPlayer))
                {
                    if (planetWalkingHandler.planetWalking && planetWalkingHandler.planetWalkingTarget == this && !planetWalking) Networking.LocalPlayer.Respawn();
                    else if (planetWalkingHandler.planetWalking && planetWalkingHandler.planetWalkingTarget == this) PlanetWalk();
                    else if (planetWalking) planetWalking = false;
                }
            }
        }
            
        if (warmUp && warmUpTimer > warmUpTime)
        {
            warmUp = false;
        }
        else if (warmUp) warmUpTimer += Time.deltaTime;
            

    }

    public override void PostLateUpdate()
    {
        if (Utilities.IsValid(localPlayer))
        {
            if (!warmUp && planetWalkingHandler.planetWalking && planetWalkingHandler.planetWalkingTarget != this)
            {
                FollowPlanetWalkTransform();
            }
        }
    }

    #endregion

    #region VRChat Callbacks

    public override void OnPlayerRespawn(VRCPlayerApi player)
    {
        if (player.isLocal && planetWalkingHandler.planetWalking && planetWalkingHandler.planetWalkingTarget == this && !firstRespawn)
        {
            firstRespawn = true;
            SetupPlanetWalk();
            planetWalkingHandler.MovePlatform();
            Debug.LogFormat("{0} Respawning local player -- setting up planet walk and triggering second respawn.", name);
            Networking.LocalPlayer.Respawn();
        }
        else if (player.isLocal && planetWalkingHandler.planetWalking && planetWalkingHandler.planetWalkingTarget == this && firstRespawn) FinishRespawn();
    }
    #endregion

    #region Planet Walking

    public void SetPlanetTransformFromWalker()
    {
        transform.position = planetWalkPosition;
        transform.rotation = planetWalkRotation;
    }

    public void FinishRespawn()
    {
        //Vector3 playerPos = localPlayer.GetPosition();
        //SetupPlanetWalk();
        lastPosition = planetWalkingHandler.platform.transform.position;
        //localPlayer.SetVelocity(Vector3.zero);
        firstRespawn = false;
        Debug.LogFormat("{0} Second respawn -- setting player velocity to zero and ending respawn cycle.", name);
        //warmUp = true;
        //warmUpTimer = 0f;
    }

    public void SetupPlanetWalk()
    {
        Debug.LogFormat("{0}: Setting Up Planet Walk", name);
        planetWalking = true;

        preTransformWorldToLocalMatrix = transform.worldToLocalMatrix;
        planetWalkPosition = Vector3.zero + planetWalkingHandler.transform.position;
        planetWalkRotation = Quaternion.identity;
        SetPlanetTransformFromWalker();


        foreach (Transform child in transform)
        {
            if (child.name == "CompassTarget")
            {
                planetWalkingHandler.SetPlatformTarget(child);

            }
        }
    }

    public void PlanetWalk()
    {
        //Debug.LogFormat("{0}: Planet Walking", name);
        preTransformWorldToLocalMatrix = transform.worldToLocalMatrix;
        Vector3 playerPos = localPlayer.GetPosition();
        Vector3 movement = playerPos - lastPosition;
        movement.y = 0f;
        if (movement.magnitude < 0.001f)
        {
            SetPlanetTransformFromWalker();
            return;
        }

        //Debug.LogFormat("{0}: Movement magnitude: {1}, player position: {2}, last position: {3}, planet walk position: {4}", name, movement.magnitude, playerPos, lastPosition, planetWalkPosition);
        float angle = movement.magnitude * (180f / Mathf.PI) / (playerPos - planetWalkPosition).magnitude;

        Vector3 rotationAxis = Vector3.Cross(Vector3.up, movement).normalized;
        planetWalkRotation = Quaternion.Euler(rotationAxis * -angle) * planetWalkRotation;

        lastPosition = playerPos;

        playerPos.y = planetWalkPosition.y;
        planetWalkPosition = playerPos;

        SetPlanetTransformFromWalker();
    }

    void FollowPlanetWalkTransform()
    {
        if (planetWalkingHandler.planetWalkingTarget != null)
        {
            Vector3 tempPos = planetWalkingHandler.planetWalkingTarget.preTransformWorldToLocalMatrix.MultiplyPoint(transform.position);
            Quaternion tempRot = planetWalkingHandler.planetWalkingTarget.preTransformWorldToLocalMatrix.rotation * transform.rotation;

            tempPos = planetWalkingHandler.planetWalkingTarget.transform.localToWorldMatrix.MultiplyPoint(tempPos);
            tempRot = planetWalkingHandler.planetWalkingTarget.transform.localToWorldMatrix.rotation * tempRot;

            transform.SetPositionAndRotation(tempPos, tempRot);
        }
    }

    public void ResetCollision()
    {

        Debug.LogFormat("{0}: Resetting collision flag.", name);
        planetCollided = false;
        if (followerTarget != null)
            followerTarget.ResetCollision();
    }
    #endregion
}