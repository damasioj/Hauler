using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.MLAgents;
using UnityEngine;

public class HaulerAcademy : MonoBehaviour
{
    Academy haulerAcademy;
    BaseTarget target;
    ObstacleManager obstacleManager;
    //Goal goal;
    List<Collider> boundaries;
    List<Obstacle> obstacles;
    
    void Awake()
    {
        haulerAcademy = Academy.Instance;
        Academy.Instance.OnEnvironmentReset += EnvironmentReset;
    }

    void Start()
    {
        obstacleManager = gameObject.AddComponent<ObstacleManager>();
        target = GetComponentInChildren<BaseTarget>();
        boundaries = GetBoundaries();
        obstacles = GetObstacles();
    }

    public void EnvironmentReset()
    {
        obstacleManager.ResetObstacles();
        
        goal.Reset();
        SetResourceRequirements();
        SetAgentTarget();
    }

    private List<Collider> GetBoundaries()
    {
        return GetComponentsInChildren<Collider>().Where(c => c.CompareTag("boundary")).ToList();
    }

    private List<Obstacle> GetObstacles()
    {
        return GetComponentsInChildren<Obstacle>().ToList();
    }
}
