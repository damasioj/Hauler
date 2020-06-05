using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using UnityEngine;

public class BaseTarget : MonoBehaviour
{
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
        transform.position = new Vector3(originalPos.x, originalPos.y, originalPos.z);
        rBody.angularVelocity = Vector3.zero;
        rBody.velocity = Vector3.zero;
    }

    private void OnTriggerEnter(Collider other)
    {
        // target is lost in boundary
        if (other.CompareTag("boundary"))
        {
            agent.MarkTaskDone(TaskEndReason.Failed);
        }
    }
}
