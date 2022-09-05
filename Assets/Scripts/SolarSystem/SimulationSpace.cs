using UdonSharp;
using VRC.SDKBase;
using VRC.Udon;
using UnityEngine;

public class SimulationSpace : UdonSharpBehaviour
{
    public GravitationalObject sun = null;
    public GravitationalObject[] gravitationalObjectList = null;
    public GameObject planetPrefab;
    public GameObject dottedLinePrefab;
    public Material planetMaterial;
    public bool showTrails = true;
    [Range(1, 10000)]
    public int nSimsPerSecond = 100;
    [Range(-0.1f, 0.1f)]
    [UdonSynced] public float timeStep = 0.01f;
    public bool randomizeChildPositions = true;
    private bool physicsRunning = false;
    private int nSteps = 0;
    private int nSimSteps = 0;
    private int nChildren = 0;
    private float totalSimTime = 0f;
    private float logTime = 1f;
    private float logTimer = 0f;
    private float fixedDeltaTime = 0f;

    public float maxStartVelocity = 10f;
    public float maxStartAngularVelocity = 100f;
    public float maxStartPosition = 50f;
    public float maxStartMass = 10f;

    public float gravitationalConstant = 6.673f;
    [UdonSynced] public float gravitationMultiplier = 1;
    [UdonSynced] public bool autoSync = false;

    public float timeSpentCalculatingDistanceVectors = 0f;
    public float timeSpentCalculatingForceVectors = 0f;
    public float timeSpentCalculatingVertexColors = 0f;

    public float throwReductionFactor = 0.2f;

    private float warmUpTimer = 0f;
    private float warmUpTime = 4.75f;
    private bool warmUp = true;

    public bool planetWalking = false;
    public GravitationalObject planetWalkingTarget = null;

    public Transform platform = null;
    public Transform platformLocationTarget;

    public bool planetTransition = false;

    public Origin origin;

    // Start is called before the first frame update
    void Start()
    {
        fixedDeltaTime = 1f / nSimsPerSecond;
        Time.fixedDeltaTime = fixedDeltaTime;
    }

    public void ToggleAutoSync()
    {
        autoSync = !autoSync;
    }

    public void Sync()
    {
        SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "CallSerialization");
        foreach (GravitationalObject gravitationalObject in gravitationalObjectList)
        {
            gravitationalObject.SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "CallSerialization");
        }
    }

    public void CallSerialization()
    {
        RequestSerialization();
    }

    public void EnablePlanetWalk(GravitationalObject gravitationalObject)
    {
        planetWalkingTarget = gravitationalObject;
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

    public void SetPlanetWalkTarget(GravitationalObject gravitationalObject)
    {
        planetWalkingTarget = gravitationalObject;
        origin._EnterRoom(gravitationalObject.transform);
    }

    //IEnumerator RunPhysics()
    //{
    //    while (physicsRunning)
    //    {
    //        CheckChildren();
    //        float executionTime = GravitationalObject.RunSimulationStep();
    //        //Debug.LogFormat("Physics run in {0}s", executionTime);
    //        GravitationalObject.SetTimeStep(timeStep);
    //        nSteps += 1;
    //        totalSimTime += executionTime;
    //        yield return null;
    //    }
    //}

    void RunPhysics()
    {
        CheckChildren();
        float executionTime = RunSimulationStep();
        //Debug.LogFormat("Physics run in {0}s", executionTime);
        nSteps += 1;
        totalSimTime += executionTime;
    }

    public void ResetTimeCounters()
    {
        timeSpentCalculatingDistanceVectors = 0f;
        timeSpentCalculatingForceVectors = 0f;
        timeSpentCalculatingVertexColors = 0f;
    }

    public float RunSimulationStep()
    {
        float t1 = Time.realtimeSinceStartup;
        foreach (GravitationalObject gravitationalObject in gravitationalObjectList)
        {
            gravitationalObject.UpdatePhysics();
        }

        return Time.realtimeSinceStartup - t1;
    }

    private void Update()
    {
        //if (!physicsRunning)
        //{
        //    physicsRunning = true;
        //    StartCoroutine(RunPhysics());
        //}
        
        fixedDeltaTime = 1f / nSimsPerSecond;

        logTimer += Time.deltaTime;
        if (logTimer >= logTime)
        {
            float tS = 1000;
            string timeUnits = "ms";
            //Debug.LogFormat("{1} sim steps run in {2:0.000}{0} | average sim time: {3:0.000}{0} | distance vector calculation time: {4:0.000}{0} | force vector calculation time {5:0.000}{0} | total vector calculation time {6:0.000}{0} | vertex color calculation time {7:0.000}{0}",
            //    timeUnits, nSteps, totalSimTime * tS, (totalSimTime / nSteps) * tS, timeSpentCalculatingDistanceVectors * tS,
            //    timeSpentCalculatingForceVectors * tS, (timeSpentCalculatingDistanceVectors + timeSpentCalculatingForceVectors) * tS, timeSpentCalculatingVertexColors * tS);

            nSteps = 0;
            totalSimTime = 0f;
            logTimer = 0f;
            ResetTimeCounters();
        }

        if (warmUp && warmUpTimer >= warmUpTime)
        {
            warmUp = false;
            Sync();
        }
        else if (warmUp) warmUpTimer += Time.deltaTime;
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

    private void FixedUpdate()
    {
        if (Time.fixedDeltaTime != fixedDeltaTime)
        {
            Time.fixedDeltaTime = fixedDeltaTime;
        }
        RunPhysics();
    }

    //void FixedUpdate()
    //{
    //    float executionTime = GravitationalObject.RunSimulationStep();
    //    Debug.LogFormat("Physics run in {0}s", executionTime);
    //    GravitationalObject.SetTimeStep(timeStep);
    //}

    public void ResetPlanets()
    {
        foreach (GravitationalObject gravitationalObject in gravitationalObjectList)
        {
            gravitationalObject.SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "ResetPlanet");
        }
    }

    public void ToggleTrails()
    {
        showTrails = !showTrails;
        foreach (GravitationalObject gravitationalObject in gravitationalObjectList)
        {
            if (showTrails && gravitationalObject.physicsActive)
            {
                gravitationalObject.EnableTrail();
            }
            else if (!showTrails)
            {
                gravitationalObject.DisableTrail();
            }
        }
    }
    
    void CheckChildren()
    {
        if (transform.childCount != nChildren)
        {
            GravitationalObject[] gravitationalObjects = new GravitationalObject[transform.childCount];
            int i = 0;
            //Debug.LogFormat("Checking {0} children", transform.childCount);
            foreach (Transform child in transform)
            {
                //Debug.LogFormat("Checking {0}, iteration {1}", child.name, i);
                if (child.gameObject.HasComponent<GravitationalObject>())
                {
                    GravitationalObject gravitationalObject = child.GetComponent<GravitationalObject>();
                    //Debug.LogFormat("{0} has component, setting in list", child.name);

                    if (!gravitationalObject.hasRunPhysics && gravitationalObject.randomizeStart)
                    {

                        Color randomColor = Random.ColorHSV();
                        //ParticleSystem ps = gravitationalObject.GetComponentInChildren<ParticleSystem>();
                        //ps.startColor = randomColor;

                        float mass = Random.Range(0.1f, maxStartMass);
                        child.localScale = Vector3.one * mass;

                        if (randomizeChildPositions)
                        {
                            child.position = new Vector3(Random.Range(-maxStartPosition, maxStartPosition), Random.Range(-maxStartPosition, maxStartPosition), Random.Range(-maxStartPosition, maxStartPosition));
                        }

                        gravitationalObject.mass = mass;
                        if (sun)
                        {
                            Vector3 tangent = Vector3.Cross(sun.GetDistanceVector(gravitationalObject.position), Vector3.up);
                            tangent.Normalize();
                            gravitationalObject.velocity = tangent * Random.Range(-maxStartVelocity, maxStartVelocity);
                            gravitationalObject.transform.localRotation = new Quaternion(Random.Range(0, 1), Random.Range(0, 1), Random.Range(0, 1), Random.Range(0, 1));
                            gravitationalObject.angularVelocity = new Vector3(Random.Range(-maxStartAngularVelocity, maxStartAngularVelocity), Random.Range(-maxStartAngularVelocity, maxStartAngularVelocity), Random.Range(-maxStartAngularVelocity, maxStartAngularVelocity));
                        }
                        else
                        {
                            gravitationalObject.velocity = new Vector3(Random.Range(-maxStartVelocity, maxStartVelocity), Random.Range(-maxStartVelocity, maxStartVelocity), Random.Range(-maxStartVelocity, maxStartVelocity));
                        }
                    }

                    gravitationalObjects[i] = gravitationalObject;
                    i++;
                }

            }
            gravitationalObjectList = gravitationalObjects;
            nChildren = transform.childCount;
        }
    }

    public void RemoveFromGravitationalObjectList(GravitationalObject gravitationalObject)
    {
        GravitationalObject[] tempList = new GravitationalObject[gravitationalObjectList.Length - 1];
        int j = 0;
        for (int i = 0; i + 1 < gravitationalObjectList.Length; i++)
        {
            if (gravitationalObjectList[i] != gravitationalObject)
            {
                tempList[j] = gravitationalObjectList[i];
                j++;
            }
        }
        gravitationalObjectList = tempList;
    }
}
