using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using UnityEngine;

public class BaseTarget : MonoBehaviour
{
    public Vector2 X, Z;
    [HideInInspector] public HaulerAgent agent;
    Rigidbody rBody;
    Vector3 originalPos;

    // Start is called before the first frame update
    void Start()
    {
        originalPos = new Vector3(transform.position.x, transform.position.y, transform.position.z);
        rBody = GetComponentInChildren<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ResetPosition()
    {
        if (X.x + X.y + Z.x + Z.y != 0)
        {
            // reset location
            float x = Random.Range(X.x, X.y);
            float z = Random.Range(Z.x, Z.y);

            transform.position = new Vector3(x, originalPos.y, z);
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
