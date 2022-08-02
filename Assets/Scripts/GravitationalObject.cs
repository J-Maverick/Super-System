using System.Runtime.InteropServices.WindowsRuntime;
using System.Xml.Serialization;
using System.Xml.Linq;
using System.Text.RegularExpressions;
using System.Net.Security;
using System.Data.Common;
using System.Data;
using System.Collections;
using System.Collections.Generic;
using UdonSharp;
using VRC.SDKBase;
using VRC.Udon;
using UnityEngine;

public class GravitationalObject : UdonSharpBehaviour
{
    public SimulationSpace simulationSpace;
    public float mass = 1f;
    public Vector3 previousFramePosition = Vector3.zero;
    [UdonSynced] public Vector3 position = Vector3.zero;
    public Vector3 acceleration = Vector3.zero;
    public Vector3 velocity = Vector3.zero;
    public Vector3 angularVelocity = Vector3.zero;
    public bool moveable = true;
    public bool staticPosition = false;

    public bool randomizeStart = true;
    public bool hasRunPhysics = false;

    private Vector3 gravitationalForce = Vector3.zero;
    private Mesh mesh;
    private Vector3[] vertices;
    private Color[] colors;
    private int nVertices;
    private float radius;
    private bool instantiated = false;

    private bool physicsActive = true;

    public bool follower = false;
    public GravitationalObject followerTarget = null;

    private bool localPlayerIsOwner = false;
    private bool ownerIsMaster = false;
    

    void Start()
    {
        simulationSpace = GetComponentInParent<SimulationSpace>();
        position = transform.localPosition;
        previousFramePosition = transform.localPosition;
        mesh = GetComponent<MeshFilter>().mesh;
        nVertices = mesh.vertices.Length;
        colors = new Color[nVertices];
        radius = mesh.vertices[0].magnitude;
        instantiated = true;
        Debug.LogFormat("{0}: start velocity: {1} | start position: {2}", this.name, velocity, position);
    }

    public void OnPickup()
    {
        physicsActive = false;
    }

    public override void Interact()
    {
        base.Interact();
    }

    public void OnDrop()
    {
        physicsActive = true;
    }

    // TODO: Add radiation and temperature
    public void UpdatePhysics()
    {
        if (instantiated && !follower)
        {
            if (physicsActive)
            {
                SetGravitationalForce();
                SetAcceleration();
                SetVelocity();
            }
            if (!staticPosition)
            {
                SetPosition();
                SetRotation();
            }
            if (!hasRunPhysics)
            {
                hasRunPhysics = true;
            }
            //Debug.LogFormat("{0} | Gravitational Force: {1}", this.name, gravitationalForce);
        }
    }

    void LateUpdate()
    {
        if (instantiated && !follower)
        {
            if (!physicsActive)
            {
                GrabOverride();
            }
            UpdatePosition();
            //SetForceColor();
        }
        else if (follower)
        {
            transform.localPosition = followerTarget.position;
            transform.localRotation = followerTarget.transform.localRotation;
        }
    }

    private void GrabOverride()
    {
        velocity = simulationSpace.throwReductionFactor * (transform.localPosition - previousFramePosition) / Time.deltaTime;
        acceleration = Vector3.zero;
        gravitationalForce = Vector3.zero;
    }

    private void SetAcceleration()
    {
        acceleration = gravitationalForce / mass;
    }

    private void SetVelocity()
    {
        //Debug.LogFormat("{0} position: {1} | velocity: {2} | acceleration: {3} | force: {4}", this.name, position, velocity, acceleration, gravitationalForce);
        velocity += acceleration * simulationSpace.timeStep;
        //Debug.LogFormat("{0} updated velocity: {1}", this.name, velocity);
    }

    private void SetPosition()
    {
        //Debug.LogFormat("{0} position: {1} | velocity: {2} | acceleration: {3} | force: {4}", this.name, position, velocity, acceleration, gravitationalForce);
        position += velocity * simulationSpace.timeStep;
        //Debug.LogFormat("{0} updated position: {1}", this.name, position);
    }

    public void UpdatePosition()
    {
        if (moveable && transform.localPosition != previousFramePosition && !physicsActive)
        {
            position = transform.localPosition;
        }
        else if (!physicsActive)
        {
            position = transform.localPosition;
        }
        else
        {
            transform.localPosition = position;
        }
        previousFramePosition = transform.localPosition;
    }

    // TODO: Add function to set vertex color based on temperature rather than force applied --- this is gonna be rad
    private void SetForceColor()
    {
        float t1 = Time.realtimeSinceStartup;
        vertices = mesh.vertices;
        //Debug.LogFormat("{0}: Vert 0: {1}", transform.name, vertices[0]);
        for (int i = 0; i < nVertices; i++)
        {
            Vector3 diff = gravitationalForce.normalized - ((transform.TransformPoint(vertices[i]) - transform.parent.position) / transform.parent.localScale.x - transform.localPosition).normalized;
            colors[i] = Color.LerpUnclamped(Color.red, Color.blue, diff.magnitude / 2);

        }
        mesh.colors = colors;
        simulationSpace.timeSpentCalculatingVertexColors += Time.realtimeSinceStartup - t1;
    }

    private void SetRotation()
    {
        Quaternion rot = transform.localRotation;

        transform.localRotation = rot * Quaternion.Euler(angularVelocity * simulationSpace.timeStep);
    }

    //TODO remove redundant force calculations by cumulating force in series -- possibly move to compute shader and just let the GPU figure it out
    //http://www.scholarpedia.org/article/N-body_simulations_(gravitational)#Direct_methods (1) F⃗ i=−∑j≠iGmimj(r⃗ i−r⃗ j) / |ri→−rj→|3−∇⃗ ⋅ϕext(r⃗ i) -- omit external potential because yeet
    private void SetGravitationalForce()
    {
        Vector3 distanceVector = Vector3.zero;
        gravitationalForce = Vector3.zero;
        foreach (GravitationalObject gravitationalObject in simulationSpace.gravitationalObjectList)
        {
            if (gravitationalObject != this && gravitationalObject.instantiated)
            {
                float t1 = Time.realtimeSinceStartup;
                distanceVector = gravitationalObject.GetDistanceVector(position);
                if (distanceVector.magnitude <= (transform.localScale.x + gravitationalObject.transform.localScale.x) / 2)
                {
                    Debug.LogFormat("{0}: Distance Vector Magnitude: {1} | Global Scale: {8} | Local Scale: {2} | Colliding Object Local Scale: {3} | Object Local Position: {4} | Colliding Object Local Position: {5} | Object Global Position: {6} | Colliding Object Global Position: {7}",
                        this.name, distanceVector.magnitude, transform.localScale.x, gravitationalObject.transform.localScale.x, transform.localPosition, gravitationalObject.transform.localPosition, transform.localPosition, gravitationalObject.transform.localPosition, simulationSpace.transform.localScale);
                    PlanetCollision(gravitationalObject);
                }
                else
                {
                    float t2 = Time.realtimeSinceStartup;
                    simulationSpace.timeSpentCalculatingDistanceVectors += t2 - t1;

                    gravitationalForce -= mass * gravitationalObject.GetMass() * distanceVector / Mathf.Pow(distanceVector.magnitude, 3);
                    float t3 = Time.realtimeSinceStartup;
                    simulationSpace.timeSpentCalculatingForceVectors += t3 - t2;
                }
            }
        }
        float t4 = Time.realtimeSinceStartup;
        gravitationalForce *= simulationSpace.gravitationalConstant * simulationSpace.gravitationMultiplier;
        float t5 = Time.realtimeSinceStartup;
        simulationSpace.timeSpentCalculatingForceVectors += t5 - t4;
    }

    public float GetMass()
    {
        return mass;
    }

    public Vector3 GetDistanceVector(Vector3 objectPosition)
    {
        return objectPosition - position;
    }

    public void PlanetCollision(GravitationalObject collisionGravObject)
    {
        Vector3 planetMomentum = GetMomentum();
        Vector3 collisionMomentum = collisionGravObject.GetMomentum();

        if (collisionGravObject.GetMass() >= mass)
        {
            collisionGravObject.AddMomentum(this);
            Destroy(this.gameObject);
        }
        else if (collisionGravObject.GetMass() < mass)
        {
            AddMomentum(collisionGravObject);
            Destroy(collisionGravObject.gameObject);
        }
    }

    public void AddMomentum(GravitationalObject collisionObject)
    {
        Debug.LogFormat("{0}: Adding momentum from {1} | starting velocity: {2} | starting mass: {3} | colliding velocity: {4} | colliding mass {5}", this.name, collisionObject.name, velocity, mass, collisionObject.velocity, collisionObject.GetMass());
        velocity = (collisionObject.GetMomentum() + GetMomentum()) / (collisionObject.GetMass() + mass);
        mass += collisionObject.GetMass();
        float scaleFactor = Mathf.Pow(Mathf.Pow(transform.localScale.x, 3f) + Mathf.Pow(collisionObject.transform.localScale.x, 3f), 1f / 3f);
        transform.localScale = Vector3.one * scaleFactor;
        Debug.LogFormat("{0}: Added momentum from {1} | new velocity: {2} | new mass: {3} | new scale: {4} | scale factor: {5}", this.name, collisionObject.name, velocity, mass, transform.localScale, scaleFactor);
    }

    public Vector3 GetMomentum()
    {
        return velocity * mass;
    }

    private void OnDestroy()
    {
        Debug.LogFormat("{0} Destroyed", this.name);
        simulationSpace.RemoveFromGravitationalObjectList(this);
    }
}
