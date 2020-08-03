using System.Collections.Generic;
using System.Linq;
using Unity.MLAgents;
using UnityEngine;

public class HaulerAcademy : MonoBehaviour
{
    Academy haulerAcademy;
    HaulerAgent hauler;
    BaseTarget activeTarget;
    List<BaseTarget> targets;
    Goal goal;
    ObstacleManager obstacleManager;
    
    void Awake()
    {
        haulerAcademy = Academy.Instance;
        Academy.Instance.OnEnvironmentReset += EnvironmentReset;
    }

    void Start()
    {
        obstacleManager = gameObject.AddComponent<ObstacleManager>();
        hauler = GetComponentInChildren<Agent>() as HaulerAgent;
        targets = GetComponentsInChildren<BaseTarget>().ToList();
        goal = GetComponentInChildren<Goal>();

        targets.ForEach(t => t.agent = hauler);
        goal.agent = hauler;
    }

    public void EnvironmentReset()
    {
        SetTarget();
        activeTarget.Reset();
        goal.Reset(activeTarget.transform.localPosition);
        obstacleManager.ResetObstacles();
    }

    public void SetTarget()
    {
        int index = Random.Range(0, targets.Count);

        activeTarget = targets[index];
        activeTarget.gameObject.SetActive(true);
        hauler.target = activeTarget;
        
        targets.Except(new[] { targets[index] }).ToList().ForEach(t => t.gameObject.SetActive(false));
    }
}
