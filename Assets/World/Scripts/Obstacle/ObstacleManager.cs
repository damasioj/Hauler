using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ObstacleManager : MonoBehaviour
{
    List<Obstacle> obstacles;

    void Awake()
    {
        obstacles = GetComponentsInChildren<Obstacle>().ToList();
    }

    public void ResetObstacles()
    {
        obstacles.ForEach(x => x.Reset());
    }
}
