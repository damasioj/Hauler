using System;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class HaulerAgent : Agent
{
    // predef
    [HideInInspector] public BaseTarget target;
    public Goal goal;
    public int stepsThreshold;
    public float positionRange;
    public float raycastDistance;

    // agent
    public event EventHandler EpisodeReset;
    Vector3 previousPosition;
    GameObject agentHead;
    Rigidbody rBody;
    int internalStepCount;
    bool targetRaycast;

    // target
    float lastTargetDistance;
    Rigidbody targetBody;
    Vector3 targetDimensions;

    // env
    List<bool> raycastsHit;
    List<GameObject> obstacles;
    List<Collider> checkPoints;
    bool isDoneCalled;

    Vector3[] directions = new Vector3[3] // add to raycast helper
    {
        Vector3.forward,
        //Vector3.right,
        //Vector3.left,
        new Vector3(0.5f, 0, 0.5f),
        new Vector3(-0.5f, 0, 0.5f)
    };

    private void Awake()
    {
        InitializeRaycasts(3);
        checkPoints = new List<Collider>();
    }

    void Start()
    {
        rBody = GetComponent<Rigidbody>();
        agentHead = GetComponentInChildren<SphereCollider>().gameObject;

        //OnEpisodeBegin();
    }

    void FixedUpdate()
    {
        ExecuteRaycasts();

        if (checkPoints.Count > 0) // only consider distance after hitting the box to avoid false rewards
        {
            float distance = ObjectHelper.EvaluateProximity(ref lastTargetDistance, target.gameObject, goal.gameObject);
            if (distance > 0)
            {
                internalStepCount = StepCount;
                AddReward(distance * 0.0001f);
                Debug.Log($"Reward: {GetCumulativeReward()}");
            }
        }

        if (StepCount - internalStepCount > stepsThreshold && !isDoneCalled)
        {
            isDoneCalled = true;
            SubtractReward(0.2f);
            Debug.Log($"Reward: {GetCumulativeReward()}");
            Debug.Log($"No point earned in last {stepsThreshold} steps. Restarting ...");
            EndEpisode();
        }
    }

    private void Reset()
    {
        SetReward(0f);

        // reset position
        do
        {
            transform.position = new Vector3(UnityEngine.Random.Range(-1f, 1f) * positionRange, transform.position.y, UnityEngine.Random.Range(-1f, 1f) * positionRange);
        }
        while (Physics.OverlapSphere(transform.position, 3f, 2).Length >= 1);

        rBody.angularVelocity = Vector3.zero;
        rBody.velocity = Vector3.zero;

        lastTargetDistance = 0f;
        internalStepCount = StepCount;
        isDoneCalled = false;
        checkPoints.Clear();
    }

    private void UpdateTargetData()
    {
        targetDimensions = ObjectHelper.GetDimensions(target.gameObject);
        targetBody = target.GetComponent<Rigidbody>();
    }

    private void InitializeRaycasts(int size)
    {
        raycastsHit = new List<bool>();
        obstacles = new List<GameObject>();
        
        for (int i = 0; i < size; i++)
        {
            raycastsHit.Add(false);
            obstacles.Add(null);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("checkpoint"))
        {
            if (!checkPoints.Contains(other))
            {
                checkPoints.Add(other);                
                internalStepCount = StepCount;
                AddReward(0.1f);
                Debug.Log("Reached box!");
                Debug.Log($"Reward: {GetCumulativeReward()}");
            }
        }
    }

    public override void OnEpisodeBegin()
    {
        Reset();
        OnEpisodeReset();
        UpdateTargetData();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // target data
        sensor.AddObservation(target.transform.position); //3
        sensor.AddObservation(targetBody.velocity); //3
        sensor.AddObservation(targetDimensions); //3
        sensor.AddObservation(target.transform.rotation); //4
        sensor.AddObservation(targetBody.mass); //1
        sensor.AddObservation(targetBody.drag); //1

        // goal data
        sensor.AddObservation(goal.transform.position); //3

        // Agent data
        sensor.AddObservation(transform.position); //3
        sensor.AddObservation(rBody.velocity); //3
        sensor.AddObservation(transform.rotation); // 4
        sensor.AddObservation(targetRaycast); //1

        // obstacle info
        raycastsHit.ForEach(x => sensor.AddObservation(x)); // n * 1
        for (int i = 0; i < obstacles.Count; i++)
        {
            if (obstacles[i] == null)
            {
                sensor.AddObservation(Vector3.zero);
                sensor.AddObservation(Vector3.zero);
                continue;
            }

            sensor.AddObservation(ObjectHelper.GetDimensions(obstacles[i])); // n * 3
            sensor.AddObservation(obstacles[i].transform.position); // n * 3
        }
    }

    public override void OnActionReceived(float[] vectorAction)
    {
        Move(vectorAction);
    }

    private void Move(float[] vectorAction)
    {
        Vector3 controlSignal = Vector3.zero;
        controlSignal.x = vectorAction[0];
        controlSignal.z = vectorAction[1];

        // agent is idle
        if (controlSignal.x == 0 && controlSignal.z == 0)
        {
            rBody.angularVelocity = Vector3.zero;
            rBody.velocity = Vector3.zero;
            return;
        }

        // agent is moving
        if (rBody.velocity.x > 30)
        {
            controlSignal.x = 0;
        }
        if (rBody.velocity.z > 30)
        {
            controlSignal.z = 0;
        }

        rBody.AddForce(new Vector3(controlSignal.x * 750, 0, controlSignal.z * 750));

        SetDirection();
    }

    private void SetDirection()
    {
        if (transform.position != previousPosition)
        {
            var direction = (transform.position - previousPosition).normalized;
            direction.y = 0;

            transform.rotation = Quaternion.LookRotation(direction);
            previousPosition = transform.position;
        }
    }

    /// <summary>
    /// Calls all associated academies to reset the environment.
    /// </summary>
    public void OnEpisodeReset()
    {
        EpisodeReset?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Used by exetrnal sources to mark that the agent failed or finished the task.
    /// </summary>
    public void MarkTaskDone(TaskEndReason reason)
    {
        if (!isDoneCalled)
        {
            if (reason == TaskEndReason.Failed)
            {
                isDoneCalled = true;
                SubtractReward(0.1f);
                Debug.Log($"Reward: {GetCumulativeReward()}");
                EndEpisode();
            }
            else if (reason == TaskEndReason.Finished)
            {
                isDoneCalled = true;
                SetReward(3f);
                Debug.Log($"Reward: {GetCumulativeReward()}");
                EndEpisode();
            }
        }
    }

    public void MarkCheckpointReached(Collider checkpoint)
    {
        if (!checkPoints.Contains(checkpoint))
        {
            checkPoints.Add(checkpoint);
            internalStepCount = StepCount;
            AddReward(0.1f);
            Debug.Log("Reached checkpoint!");            
            Debug.Log($"Reward: {GetCumulativeReward()}");
        }
    }

    private void ExecuteRaycasts() //refactor
    {
        // Bit shift the index of the layer (8) to get a bit mask
        int layerMask = 1 << 8;

        // This would cast rays only against colliders in layer 8.
        // But instead we want to collide against everything except layer 8. The ~ operator does this, it inverts a bitmask.
        layerMask = ~layerMask;

        for (int i = 0; i < directions.Length; i++)
        {
            if (Physics.Raycast(agentHead.transform.position, agentHead.transform.TransformDirection(directions[i]), out RaycastHit hit, raycastDistance, layerMask))
            {
                Debug.DrawRay(agentHead.transform.position, agentHead.transform.TransformDirection(directions[i]) * hit.distance, Color.red);

                ValidateRaycastCollision(hit, i);
            }
            else
            {
                Debug.DrawRay(agentHead.transform.position, agentHead.transform.TransformDirection(directions[i]) * raycastDistance, Color.white);

                raycastsHit[i] = false;
                obstacles[i] = null;
            }
        }

        // check target raycast ... we want this to be separate from others
        Vector3 downDirection = new Vector3(0, -0.3f, 1f);
        if (Physics.Raycast(agentHead.transform.position, agentHead.transform.TransformDirection(downDirection), out RaycastHit targHit, raycastDistance / 2, layerMask))
        {
            Debug.DrawRay(agentHead.transform.position, agentHead.transform.TransformDirection(downDirection) * targHit.distance, Color.red);
            ValidateRaycastCollision(targHit, 0);
        }
        else
        {
            Debug.DrawRay(agentHead.transform.position, agentHead.transform.TransformDirection(downDirection) * raycastDistance / 2, Color.white);
            targetRaycast = false;
        }
        
    }

    private void ValidateRaycastCollision(RaycastHit hit, int index)
    {
        if (hit.collider.CompareTag("obstacle"))
        {
            raycastsHit[index] = true;
            obstacles[index] = hit.collider.gameObject;
        }
        else if (hit.collider.CompareTag("target"))
        {
            targetRaycast = true;
        }
        else
        {
            raycastsHit[index] = false;
            obstacles[index] = null;
        }
    }

    public override void Heuristic(float[] actions)
    {
        actions[0] = Input.GetAxis("Horizontal");
        actions[1] = Input.GetAxis("Vertical");
    }

    private void SubtractReward(float value) 
    {
        AddReward(value * -1);
    }

    /// <summary>
    /// The base SetReward doesn't work so we use this instead.
    /// </summary>
    /// <param name="value"></param>
    new private void SetReward(float value)
    {
        SubtractReward(GetCumulativeReward());
        AddReward(value);
    }
}
