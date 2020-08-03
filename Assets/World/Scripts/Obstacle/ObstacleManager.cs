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
        obstacles.ForEach(x => x.gameObject.SetActive(false)); // this is necessary to not cause obstacles to "overlap"
        obstacles.ForEach(x => x.Reset());
    }
}
