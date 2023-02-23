using UdonSharp;
using VRC.SDKBase;
using VRC.Udon;
using UnityEngine;

public class PlanetWalkingHandler : UdonSharpBehaviour
{ 
    public PlanetFollower sun = null;
    public PlanetFollower[] planetFollowerList = null;

    private int nChildren = 0;

    public bool planetWalking = false;
    public PlanetFollower planetWalkingTarget = null;

    public Transform platform = null;
    public Transform platformLocationTarget;

    public bool planetTransition = false;

    public Origin origin;

    private VRCPlayerApi localPlayer;

    public void Start()
    {
        localPlayer = Networking.LocalPlayer;
    }

    public void SpewPlayerData()
    {
        if (Time.frameCount % 50 == 0)
        {
            Debug.LogFormat("Local Player -- Velocity: {0} | Position: {1}", Networking.LocalPlayer.GetVelocity(), Networking.LocalPlayer.GetPosition());
        }
    }

    public void RespawnFastGBJIdiotSpaghettiBabies()
    {
        if (Networking.LocalPlayer.GetVelocity().magnitude > 200f)
        {
            Debug.LogFormat("Spaghetti GBJ Protection Activated!");
            Networking.LocalPlayer.SetVelocity(Vector3.zero);
            Networking.LocalPlayer.Respawn();
        }
    }

    public void Update()
    {
        CheckChildren();
        SpewPlayerData();
        RespawnFastGBJIdiotSpaghettiBabies();
    }

    public void EnablePlanetWalk(PlanetFollower planetFollower)
    {
        planetWalkingTarget = planetFollower;
        planetTransition = true;
    }

    public void DisablePlanetWalk()
    {
        planetWalkingTarget = null;
        planetWalking = false;
    }

    public void SetPlatformTarget(Transform targetTransform)
    {
        platformLocationTarget = targetTransform;
    }

    public void SetPlanetWalkTarget(PlanetFollower planetFollower)
    {
        planetWalkingTarget = planetFollower;
        origin._EnterRoom(planetFollower.transform);
    }

    public override void PostLateUpdate()
    {
        if (planetTransition)
        {
            Networking.LocalPlayer.Respawn();
            planetTransition = false;
        }


        if (platform != null && planetWalkingTarget != null)
        {
            if (planetWalkingTarget.planetIsKill)
            {
                EnablePlanetWalk(sun);
                sun.SetupPlanetWalk();
            }
            platform.SetPositionAndRotation(platformLocationTarget.position, platformLocationTarget.rotation);
            if (planetWalkingTarget != null && planetWalkingTarget.planetCollided)
            {
                Debug.LogFormat("{0} Collided, respawning local player.", planetWalkingTarget.name);
                platform.SetPositionAndRotation(platformLocationTarget.position, platformLocationTarget.rotation);
                Networking.LocalPlayer.Respawn();
                planetWalkingTarget.ResetCollision();
            }
        }

    }

    public void MovePlatform()
    {
        platform.SetPositionAndRotation(platformLocationTarget.position, platformLocationTarget.rotation);
    }

    void CheckChildren()
    {
        if (transform.childCount != nChildren)
        {
            PlanetFollower[] planetFollowers = new PlanetFollower[transform.childCount];
            int i = 0;
            //Debug.LogFormat("Checking {0} children", transform.childCount);
            foreach (Transform child in transform)
            {
                //Debug.LogFormat("Checking {0}, iteration {1}", child.name, i);
                if (child.gameObject.HasComponent<PlanetFollower>())
                {
                    PlanetFollower planetFollower = child.GetComponent<PlanetFollower>();
                    //Debug.LogFormat("{0} has component, setting in list", child.name);

                    planetFollowers[i] = planetFollower;
                    i++;
                }

            }
            planetFollowerList = planetFollowers;
            nChildren = transform.childCount;
        }
    }

}
