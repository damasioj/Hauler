using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class HaulerAgent : Agent
{
    public BaseTarget target;
    public Goal goal;
    
    HaulerAcademy academy;
    Vector3 _lastPosition;
    Rigidbody rBody;
    Rigidbody targetBody;

    bool isDoneCalled;

    // Start is called before the first frame update
    void Start()
    {
        isDoneCalled = false;
        _lastPosition = gameObject.transform.position;
        
        rBody = GetComponent<Rigidbody>();
        academy = GetComponentInParent<HaulerAcademy>();
        targetBody = target.GetComponentInChildren<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        // entered bridge
        // entered boundary
    }

    public override void OnEpisodeBegin()
    {
        SetReward(0f);
        //internalStepCount = 0;

        rBody.angularVelocity = Vector3.zero;
        rBody.velocity = Vector3.zero;
        //transform.localPosition = new Vector3(0, 1, 0);

        academy.EnvironmentReset(); // TODO : find a way to refactor this ... agent shouldn't call academy functions
        isDoneCalled = false;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // boundaries
        //BoundaryLimits.Values.ToList().ForEach(b => sensor.AddObservation(b)); //4

        // target location
        sensor.AddObservation(target.transform.position); //3
        sensor.AddObservation(targetBody.mass); //1
        sensor.AddObservation(targetBody.drag); //1
        sensor.AddObservation(targetBody.velocity); //3

        // goal info
        sensor.AddObservation(goal.transform.localPosition.x); //1
        sensor.AddObservation(goal.transform.localPosition.z); //1

        // Agent data
        sensor.AddObservation(HasResource); //1
        sensor.AddObservation(transform.localPosition.x); //1
        sensor.AddObservation(transform.localPosition.z); //1
        sensor.AddObservation(rBody.velocity.x); //1
        sensor.AddObservation(rBody.velocity.z); //1
        sensor.AddObservation((int)CurrentState); // 1
        sensor.AddObservation(atResource); // 1
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
        var direction = (transform.position - _lastPosition).normalized;

        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(-direction), 0.15F);
    }

    public override void Heuristic(float[] actions)
    {
        actions[0] = Input.GetAxis("Horizontal");
        actions[1] = Input.GetAxis("Vertical");
    }
}
