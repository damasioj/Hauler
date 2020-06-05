using UnityEngine;

public class Obstacle : MonoBehaviour
{
    public float maxX, minX, maxZ, minZ;
    private Vector3 pos;

    private void Start()
    {
        pos = gameObject.transform.position;
    }

    public virtual void Reset()
    {
        if (maxX + maxZ + minX + minZ != 0)
        {
            // reset location
            float x = Random.Range(minX, maxX);
            float z = Random.Range(minZ, maxZ);

            pos = new Vector3(x, pos.y, z);
        }
    }
}
