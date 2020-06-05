using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Goal : MonoBehaviour
{
    [HideInInspector] public HaulerAgent agent;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("target"))
        {
            agent.MarkTaskDone(TaskEndReason.Finished);
        }
    }
}
