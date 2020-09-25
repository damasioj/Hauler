using UnityEngine;

public class RaycastSensor
{
    public RaycastType Type { get; set; }
    public float Distance { get; set; }
    public bool HitObject { get; set; }
    public GameObject Obstacle { get; set; }
    public Vector3 Direction { get; set; }

    public RaycastSensor(RaycastType type, Vector3 direction, float distance, bool hitObject = false)
    {
        Type = type;
        Direction = direction;
        Distance = distance;
        HitObject = hitObject;
    }
}
