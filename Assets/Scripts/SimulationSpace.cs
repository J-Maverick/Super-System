﻿using UdonSharp;
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
    public float timeStep = 0.01f;
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
    public float gravitationMultiplier = 1;

    public float timeSpentCalculatingDistanceVectors = 0f;
    public float timeSpentCalculatingForceVectors = 0f;
    public float timeSpentCalculatingVertexColors = 0f;

    public float throwReductionFactor = 0.2f;

    public float tfuck = 0f;

    // Start is called before the first frame update
    void Start()
    {
        fixedDeltaTime = 1f / nSimsPerSecond;
        Time.fixedDeltaTime = fixedDeltaTime;
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
        float t1 = Time.realtimeSinceStartup;
        CheckChildren();
        tfuck += Time.realtimeSinceStartup - t1;
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
        tfuck = 0f;
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
            Debug.LogFormat("{1} sim steps run in {2:0.000}{0} | average sim time: {3:0.000}{0} | distance vector calculation time: {4:0.000}{0} | force vector calculation time {5:0.000}{0} | total vector calculation time {6:0.000}{0} | vertex color calculation time {7:0.000}{0}",
                timeUnits, nSteps, totalSimTime * tS, (totalSimTime / nSteps) * tS, timeSpentCalculatingDistanceVectors * tS,
                timeSpentCalculatingForceVectors * tS, (timeSpentCalculatingDistanceVectors + timeSpentCalculatingForceVectors) * tS, timeSpentCalculatingVertexColors * tS);

            Debug.LogFormat("Time spent checking babies: {0}s", tfuck);
            nSteps = 0;
            totalSimTime = 0f;
            logTimer = 0f;
            ResetTimeCounters();
        }
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

                    if (showTrails | !child.GetChild(0).gameObject.HasComponent<ParticleSystem>()) child.GetChild(0).gameObject.SetActive(true);
                    else child.GetChild(0).gameObject.SetActive(false);

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
