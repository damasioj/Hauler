using UnityEngine;

public class Obstacle : MonoBehaviour
{
    public float maxX, minX, maxZ, minZ;
    private float posY;

    private void Start()
    {
        posY = gameObject.transform.position.y;
    }

    public virtual void Reset()
    {
        if (maxX + maxZ + minX + minZ != 0)
        {
            // reset location
            float x = Random.Range(minX, maxX);
            float z = Random.Range(minZ, maxZ);

            gameObject.transform.position = new Vector3(x, posY, z);
        }
    }
}
