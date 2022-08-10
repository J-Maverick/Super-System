using UdonSharp;
using VRC.SDKBase;
using VRC.Udon;
using UnityEngine;
using VRC.Udon.Common;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class GravitationalObject : UdonSharpBehaviour
{
    public SimulationSpace simulationSpace;
    public float mass = 1f;
    public Vector3 previousFramePosition = Vector3.zero;
    [UdonSynced] public Vector3 position = Vector3.zero;
    public Vector3 acceleration = Vector3.zero;
    [UdonSynced] public Vector3 velocity = Vector3.zero;
    [UdonSynced] public Vector3 angularVelocity = Vector3.zero;

    [UdonSynced] private Vector3 initialPosition = Vector3.zero;
    [UdonSynced] private Vector3 initialVelocity = Vector3.zero;
    [UdonSynced] private Vector3 initialAngularVelocity = Vector3.zero;
    [UdonSynced] private Vector3 initialScale = Vector3.zero;
    [UdonSynced] private float initialMass;
    [UdonSynced] private int masterID;

    public float syncDistance = 2f;

    public Vector3 deathPosition = new Vector3(0, -10000000, 0);

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

    [UdonSynced] public bool physicsActive = true;

    public bool follower = false;
    public GravitationalObject followerTarget = null;

    private bool ownerIsMaster = false;
    private bool isOwner = false;

    private float syncTime = 0f;
    private float syncTimer = 0f;
    private float syncRate = 1f;

    private bool warmUp = true;
    private float warmUpTime = .1f;
    private float warmUpTimer = 0f;

    public bool spewDataOnPickup = false;
    public bool spewDataOnDrop = false;

    public bool autoSync = false;

    private Animator animator;

    void Start()
    {

        if (Networking.LocalPlayer.isMaster)
        {
            ownerIsMaster = true;
            isOwner = true;
            position = transform.localPosition;
            initialPosition = position;
            initialVelocity = velocity;
            initialAngularVelocity = angularVelocity;
            initialMass = mass;
            initialScale = transform.localScale;
            masterID = Networking.LocalPlayer.playerId;
        }
        simulationSpace = GetComponentInParent<SimulationSpace>();
        position = transform.localPosition;
        previousFramePosition = position;
        //mesh = GetComponent<MeshFilter>().mesh;
        //nVertices = mesh.vertices.Length;
        //colors = new Color[nVertices];
        //radius = mesh.vertices[0].magnitude;
        Debug.LogFormat("{0}: start velocity: {1} | start position: {2}", this.name, velocity, position);
        instantiated = true;
        syncTime = Random.Range(0f, 2f);
        if (gameObject.HasComponent<Animator>())
        {
            animator = GetComponent<Animator>();
            EnableTrail();
        }
    }

    public void CallSerialization()
    {
        RequestSerialization();
    }

    public override void OnPickup()
    {
        physicsActive = false;
        Networking.SetOwner(Networking.LocalPlayer, gameObject);
        animator.SetBool("EmitParticles", false);
        if (spewDataOnPickup)
        {
            SpewData();
        }
        RequestSerialization();
    }
    
    public override void OnPlayerJoined(VRCPlayerApi player)
    {
        ResetSync();
    }

    public override void OnDrop()
    {
        physicsActive = true;
        animator.SetBool("EmitParticles", true);
        if (spewDataOnDrop)
        {
            SpewData();
        }
        RequestSerialization();
    }

    // TODO: Add radiation and temperature
    public void UpdatePhysics()
    {
        if (!warmUp && instantiated && !follower)
        {
            if (!staticPosition)
            {
                if (physicsActive)
                {
                    SetGravitationalForce();
                    SetAcceleration();
                    SetVelocity();
                }
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
    
    public void ResetPlanet()
    {
        position = initialPosition;
        velocity = initialVelocity * Mathf.Sqrt(simulationSpace.gravitationMultiplier);
        angularVelocity = initialAngularVelocity;
        mass = initialMass;
        transform.localScale = initialScale;
        if (gameObject.HasComponent<Animator>())
        {
            EnableTrail();
        }
        physicsActive = true;
        RequestSerialization();
    }

    void ResetSync()
    {
        syncTimer = 0f;
    }

    void TriggerSync()
    {
        RequestSerialization();
    }

    public override void OnPostSerialization(SerializationResult result)
    {
        base.OnPostSerialization(result);
    }

    void LateUpdate()
    {
        if (!isOwner && Networking.GetOwner(gameObject) == Networking.LocalPlayer)
        {
            isOwner = true;
        }
        else isOwner = false;

        if (instantiated && !follower)
        {
            if (!physicsActive && isOwner)
            {
                GrabOverride();
            }
            UpdatePosition();
            //SetForceColor();
        }
        else if (follower)
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
            }
        }

        TestIfGrabbedByOwner();

        autoSync = simulationSpace.autoSync;
        if (autoSync && syncTimer >= syncTime)
        {
            TriggerSync();
            ResetSync();
        }
        
        syncTimer += Time.deltaTime;

        if (warmUp && warmUpTimer > warmUpTime)
        {
            warmUp = false;
        }
        else if (warmUp) warmUpTimer += Time.deltaTime;

    }

    private void TestIfGrabbedByOwner()
    {
        if (!physicsActive && isOwner)
        {
            transform.localScale += Vector3.one * Mathf.Cos(3 * Time.fixedTime) * 0.003f;
        }
    }

    private void GrabOverride()
    {
        velocity = simulationSpace.throwReductionFactor * (transform.localPosition - previousFramePosition) / Time.deltaTime;
        acceleration = Vector3.zero;
        gravitationalForce = Vector3.zero;
        position = transform.localPosition;
        RequestSerialization();
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
        transform.localPosition = position;
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
                if (gravitationalObject.physicsActive && distanceVector.magnitude <= (transform.localScale.x + gravitationalObject.transform.localScale.x) / 2)
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

        if (collisionMomentum.magnitude >= planetMomentum.magnitude)
        {
            collisionGravObject.AddMomentum(this);
            killPlanet();
        }
        else if (collisionMomentum.magnitude < planetMomentum.magnitude)
        {
            AddMomentum(collisionGravObject);
            collisionGravObject.killPlanet();
        }
    }

    public void killPlanet()
    {
        //Networking.Destroy(gameObject);
        //Destroy(gameObject);
        position = deathPosition;
        transform.localPosition = position;
        DisableTrail();
        physicsActive = false;

    }

    public void EnableTrail()
    {
        if (gameObject.HasComponent<Animator>())
        {
            animator.SetBool("ParticlesEnabled", true);
            EnableTrailParticles();
        }
    }

    public void DisableTrail()
    {
        if (gameObject.HasComponent<Animator>())
        {
            animator.SetBool("ParticlesEnabled", false);
            DisableTrailParticles();
        }
    }

    public void EnableTrailParticles()
    {
        if (gameObject.HasComponent<Animator>())
        {
            animator.SetBool("EmitParticles", true);
        }
    }
    public void DisableTrailParticles()
    {
        if (gameObject.HasComponent<Animator>())
        {
            animator.SetBool("EmitParticles", false);
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
        return (Vector3.one + velocity) * mass;
    }

    private void OnDestroy()
    {
        Debug.LogFormat("{0} Destroyed", this.name);
        simulationSpace.RemoveFromGravitationalObjectList(this);
    }

    private void SpewData()
    {
        Debug.LogFormat("{41}:: " +
            "simulationSpace: {0} | " +
            "mass: {1} | " +
            "previousFramePosition: {2} | " +
            "position: {3} | " +
            "acceleration: {4} | " +
            "velocity: {5} | " +
            "angularVelocity: {6} | " +
            "initialPosition: {7} | " +
            "initialVelocity: {8} | " +
            "initialAngularVelocity: {9} | " +
            "initialScale: {10} | " +
            "initialMass: {11} | " +
            "masterID: {12} | " +
            "syncDistance: {13} | " +
            "deathPosition: {14} | " +
            "moveable: {15} | " +
            "staticPosition: {16} | " +
            "randomizeStart: {17} | " +
            "hasRunPhysics: {18} | " +
            "gravitationalForce: {19} | " +
            "mesh: {20} | " +
            "vertices: {21} | " +
            "colors: {22} | " +
            "nVertices: {23} | " +
            "radius: {24} | " +
            "instantiated: {25} | " +
            "physicsActive: {26} | " +
            "follower: {27} | " +
            "followerTarget: {28} | " +
            "ownerIsMaster: {29} | " +
            "isOwner: {30} | " +
            "syncTime: {31} | " +
            "syncTimer: {32} | " +
            "syncRate: {33} | " +
            "warmUp: {34} | " +
            "warmUpTime: {35} | " +
            "spewDataOnPickup: {36} | " +
            "spewDataOnDrop: {37} | " +
            "animator: {38} | " +
            "warmUpTimer: {39} | " +
            "autoSync: {40}",
            simulationSpace,
            mass,
            previousFramePosition,
            position,
            acceleration,
            velocity,
            angularVelocity,
            initialPosition,
            initialVelocity,
            initialAngularVelocity,
            initialScale,
            initialMass,
            masterID,
            syncDistance,
            deathPosition,
            moveable,
            staticPosition,
            randomizeStart,
            hasRunPhysics,
            gravitationalForce,
            mesh,
            vertices,
            colors,
            nVertices,
            radius,
            instantiated,
            physicsActive,
            follower,
            followerTarget,
            ownerIsMaster,
            isOwner,
            syncTime,
            syncTimer,
            syncRate,
            warmUp,
            warmUpTime,
            spewDataOnPickup,
            spewDataOnDrop,
            animator,
            warmUpTimer,
            autoSync,
            name
        );
    }

            

}
