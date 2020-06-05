using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ObstacleManager : MonoBehaviour
{
    List<Obstacle> obstacles;

    // Start is called before the first frame update
    void Start()
    {
        obstacles = GetComponents<Obstacle>().ToList();
    }

    public void ResetObstacles()
    {
        obstacles.ForEach(x => x.Reset());
    }
}
