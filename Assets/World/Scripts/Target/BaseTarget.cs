using UnityEngine;
using UnityEngine.UIElements;

public abstract class BaseTarget : MonoBehaviour
{
    [HideInInspector] public HaulerAgent agent;
    public float positionRange;
    public float minMass, maxMass, minScale, maxScale, maxDrag;
    public virtual TargetType Shape { get; protected set; }
    
    Rigidbody rBody;
    Vector3 originalPos;
    int layerMask;

    void Start()
    {
        originalPos = new Vector3(transform.position.x, transform.position.y, transform.position.z);
        rBody = GetComponentInChildren<Rigidbody>();
        layerMask = 2;
    }

    public void Reset()
    {
        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
        }

        // reset scale
        if (minScale + maxScale != 0)
        {
            float scale = Random.Range(minScale, maxScale);

            if (transform.localScale.x == transform.localScale.y)
            {
                transform.localScale = new Vector3(scale, scale, scale);
            }
            else
            {
                transform.localScale = new Vector3(scale, transform.localScale.y, scale);
            }
        }

        // reset location
        float x, z;
        do
        {
            x = Random.Range(-1f, 1f) * positionRange;
            z = Random.Range(-1f, 1f) * positionRange;

            transform.position = new Vector3(x, originalPos.y, z);
        }
        while (Physics.OverlapSphere(new Vector3(x, originalPos.y, z), 3f, layerMask).Length == 1);

        // reset mass
        if (minMass + maxMass != 0)
        {
            rBody.mass = Random.Range(minMass, maxMass);
        }

        // reset drag
        if (maxDrag > 0)
        {
            rBody.drag = Random.Range(1f, maxDrag);
        }
        
        rBody.angularVelocity = Vector3.zero;
        rBody.velocity = Vector3.zero;
        transform.rotation = new Quaternion(0,0,0,0);
    }

    private void OnTriggerEnter(Collider other)
    {
        // target is lost in boundary
        if (other.CompareTag("boundary"))
        {
            agent.MarkTaskDone(TaskEndReason.Failed);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("checkpoint"))
        {
            agent.MarkCheckpointReached(other);
        }
    }
}
