using System;
using System.Collections.Generic;
using System.Linq;
using Unity.MLAgents;
using UnityEngine;

public class HaulerAcademy : MonoBehaviour
{
    HaulerAgent hauler;
    BaseTarget activeTarget;
    List<BaseTarget> targets;
    Goal goal;
    ObstacleManager obstacleManager;
    
    void Awake()
    {
        obstacleManager = gameObject.AddComponent<ObstacleManager>();
        hauler = GetComponentInChildren<Agent>() as HaulerAgent;
        targets = GetComponentsInChildren<BaseTarget>().ToList();
        goal = GetComponentInChildren<Goal>();

        Academy.Instance.OnEnvironmentReset += EnvironmentReset;
        hauler.EpisodeReset += EnvironmentReset;
    }

    void Start()
    {
        targets.ForEach(t => t.agent = hauler);
        goal.agent = hauler;
    }

    public void EnvironmentReset()
    {
        SetTarget();                
        obstacleManager.ResetObstacles();
        activeTarget.Reset();
        goal.Reset(activeTarget.transform.localPosition);
    }

    public void EnvironmentReset(object sender, EventArgs e)
    {
        EnvironmentReset();
    }

    public void SetTarget()
    {
        int index = UnityEngine.Random.Range(0, targets.Count);

        activeTarget = targets[index];
        activeTarget.gameObject.SetActive(true);        
        targets.Except(new[] { targets[index] }).ToList().ForEach(t => t.gameObject.SetActive(false));

        hauler.UpdateTarget(activeTarget);
    }
}
