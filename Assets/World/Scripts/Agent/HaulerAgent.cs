using System;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class HaulerAgent : Agent
{
    // predef
    public Goal goal;
    public int stepsThreshold;
    public float positionRange;
    public float raycastDistance;
    public float acceleration;

    // agent
    public event EventHandler EpisodeReset;
    Vector3 previousPosition;
    GameObject agentHead;
    Rigidbody rBody;
    int internalStepCount;

    // target    
    BaseTarget target;
    float lastTargetDistance;
    Rigidbody targetBody;
    Vector3 targetDimensions;

    // env
    object dataLock = new object(); // used to ensure data is not sent to model while resetting environment
    List<RaycastSensor> sensors;
    List<Collider> checkPoints;
    bool isDoneCalled;

    private void Awake()
    {
        InitializeRaycasts();
        checkPoints = new List<Collider>();
    }

    void Start()
    {
        rBody = GetComponent<Rigidbody>();
        agentHead = GetComponentInChildren<SphereCollider>().gameObject;
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
            //SubtractReward(0.2f);
            Debug.Log($"Reward: {GetCumulativeReward()}");
            Debug.Log($"No point earned in last {stepsThreshold} steps. Restarting ...");
            EndEpisode();
        }

        // multiply gravity
        if (rBody.velocity.y < 0)
        {
            rBody.AddForce(new Vector3(0, rBody.velocity.y * 0.3f, 0), ForceMode.VelocityChange);
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
        internalStepCount = 0;
        isDoneCalled = false;
        checkPoints.Clear();
    }

    private void UpdateTargetData()
    {
        targetDimensions = ObjectHelper.GetDimensions(target.gameObject);
        targetBody = target.GetComponent<Rigidbody>();
    }

    private void InitializeRaycasts()
    {
        sensors = new List<RaycastSensor>();
        
        Vector3[] directionsObstacles = new Vector3[3] 
        {
            Vector3.forward,
            new Vector3(0.5f, 0, 0.5f), // forward-right
            new Vector3(-0.5f, 0, 0.5f), // forward-left
        };

        Vector3[] directionsTargets = new Vector3[3]
        {
            new Vector3(0, -0.3f, 1f), // down
            new Vector3(0.5f, -0.3f, 1f), // down-right
            new Vector3(-0.5f, -0.3f, 1f) // down-left
        };

        foreach(Vector3 direction in directionsObstacles)
        {
            sensors.Add(new RaycastSensor(RaycastType.Obstacle, direction, raycastDistance));
        }

        foreach (Vector3 direction in directionsTargets)
        {
            sensors.Add(new RaycastSensor(RaycastType.Target, direction, raycastDistance / 3));
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
        lock (dataLock)
        {
            Reset();
            OnEpisodeReset();
        }
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        lock (dataLock)
        {
            // target data
            sensor.AddObservation(target.transform.position); //3
            sensor.AddObservation(target.transform.rotation); //4
            sensor.AddObservation(targetBody.velocity); //3
            sensor.AddObservation(targetDimensions); //3        
            sensor.AddObservation(targetBody.mass); //1
            sensor.AddObservation(targetBody.drag); //1

            // goal data
            sensor.AddObservation(goal.transform.position); //3

            // Agent data
            sensor.AddObservation(transform.position); //3
            sensor.AddObservation(rBody.velocity); //3
            sensor.AddObservation(transform.rotation); // 4

            // raycast info
            foreach(var rSensor in sensors)
            {
                sensor.AddObservation(rSensor.HitObject);

                if (rSensor.Type == RaycastType.Obstacle)
                {
                    if (rSensor.HitObject) 
                    {
                        sensor.AddObservation(ObjectHelper.GetDimensions(rSensor.Obstacle));
                        sensor.AddObservation(rSensor.Obstacle.transform.position);
                    }
                    else
                    {
                        sensor.AddObservation(Vector3.zero);
                        sensor.AddObservation(Vector3.zero);
                    }
                }
            }
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

        // agent is moving
        var scale = gameObject.transform.localScale.x;
        rBody.AddForce(new Vector3(controlSignal.x * acceleration * scale, 0, controlSignal.z * acceleration * scale));

        SetDirection();
    }

    private void SetDirection()
    {
        if (transform.position.x != previousPosition.x &&
            transform.position.z != previousPosition.z)
        {
            var direction = (transform.position - previousPosition).normalized;
            direction.y = 0;

            transform.rotation = Quaternion.LookRotation(direction);
        }

        previousPosition = transform.position;
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
        int layerMask = 1 << 8;
        layerMask = ~layerMask;

        foreach(var sensor in sensors)
        {
            if (Physics.Raycast(agentHead.transform.position, agentHead.transform.TransformDirection(sensor.Direction), out RaycastHit hit, sensor.Distance, layerMask))
            {
                Debug.DrawRay(agentHead.transform.position, agentHead.transform.TransformDirection(sensor.Direction) * hit.distance, Color.red);

                ValidateRaycastCollision(sensor, hit);
            }
            else
            {
                Debug.DrawRay(agentHead.transform.position, agentHead.transform.TransformDirection(sensor.Direction) * sensor.Distance, Color.white);

                sensor.HitObject = false;
                sensor.Obstacle = null;
            }      
        }
    }

    private void ValidateRaycastCollision(RaycastSensor sensor, RaycastHit hit)
    {
        if (hit.collider.CompareTag("obstacle") && sensor.Type == RaycastType.Obstacle)
        {
            sensor.HitObject = true;
            sensor.Obstacle = hit.collider.gameObject;
        }
        else if (hit.collider.CompareTag("target") && sensor.Type == RaycastType.Target)
        {
            sensor.HitObject = true;
        }
        else
        {
            sensor.HitObject = false;
            sensor.Obstacle = null;
        }
    }

    public void UpdateTarget(BaseTarget newTarget)
    {
        target = newTarget;
        UpdateTargetData();
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
