using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class HaulerAgent : Agent
{
    public BaseTarget target;
    public Goal goal;
    
    HaulerAcademy academy;
    Vector3 startingPosition;
    Vector3 previousPosition;
    Rigidbody rBody;
    Rigidbody targetBody;

    bool isDoneCalled;
    bool hitBridge;
    float minDistance;

    // Start is called before the first frame update
    void Start()
    {
        isDoneCalled = false;
        hitBridge = false;
        previousPosition = transform.position;
        startingPosition = new Vector3(transform.position.x, transform.position.y, transform.position.z);
        
        rBody = GetComponent<Rigidbody>();
        academy = GetComponentInParent<HaulerAcademy>();
        targetBody = target.GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        float distance = CheckDistance();

        AddReward(distance * 0.01f);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("bridge"))
        {
            if (!hitBridge)
            {
                hitBridge = true;
                AddReward(0.5f);
            }
        }
        else if (other.CompareTag("boundary"))
        {
            if (!isDoneCalled)
            {
                isDoneCalled = true;
                SubtractReward(0.1f);
                Debug.Log($"Reward: {GetCumulativeReward()}");
                EndEpisode();
            }
        }
    }

    public override void OnEpisodeBegin()
    {
        SetReward(0f);

        transform.position = startingPosition;
        rBody.angularVelocity = Vector3.zero;
        rBody.velocity = Vector3.zero;
        //transform.localPosition = new Vector3(0, 1, 0);

        academy.EnvironmentReset(); // TODO : find a way to refactor this ... agent shouldn't call academy functions
        isDoneCalled = false;
        hitBridge = false;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // target location
        sensor.AddObservation(target.transform.position); //3
        sensor.AddObservation(targetBody.mass); //1
        sensor.AddObservation(targetBody.drag); //1
        sensor.AddObservation(targetBody.velocity); //3

        // goal info
        sensor.AddObservation(goal.transform.position); //3

        // Agent data
        sensor.AddObservation(transform.position); //3
        sensor.AddObservation(rBody.velocity); //3
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
        var direction = (transform.position - previousPosition).normalized;
        direction.y = 0;

        //transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), 0.15F);
        transform.rotation = Quaternion.LookRotation(direction);
        previousPosition = transform.position;
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
                AddReward(3f);
                Debug.Log($"Reward: {GetCumulativeReward()}");
                EndEpisode();
            }
        }
    }

    private float CheckDistance()
    {
        // check if the distance between agent and goal was reduced
        return 0f;
    }

    public override void Heuristic(float[] actions)
    {
        actions[0] = Input.GetAxis("Horizontal");
        actions[1] = Input.GetAxis("Vertical");
    }

    private void SubtractReward(float value) // TODO : add to agent class
    {
        AddReward(value * -1);
    }
}
