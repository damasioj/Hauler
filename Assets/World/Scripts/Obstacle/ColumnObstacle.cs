using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ColumnObstacle : Obstacle
{
    List<GameObject> columns;
    
    void Start()
    {
        columns = gameObject.GetComponentsInChildren<GameObject>().ToList();
    }

    public override void Reset()
    {
        
    }
}
