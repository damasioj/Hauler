using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Goal : MonoBehaviour
{
    public Vector3[] locations;
    [HideInInspector] public HaulerAgent agent;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("target"))
        {
            agent.MarkTaskDone(TaskEndReason.Finished);
        }
    }

    public void Reset()
    {
        int numOfLocations = locations.Length;
        gameObject.transform.position = locations[Random.Range(0, numOfLocations)];
    }
}
