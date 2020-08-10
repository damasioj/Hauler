using Unity.Barracuda;
using UnityEngine;

public class Obstacle : MonoBehaviour
{
    public float maxX, minX, maxZ, minZ, minScale, maxScale;
    
    public virtual void Reset()
    {
        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
        }

        if (maxX + maxZ != 0 || minX + minZ != 0)
        {
            float scale = Random.Range(minScale, maxScale);

            // set scale
            if (transform.localScale.x == transform.localScale.z)
            {
                transform.localScale = new Vector3(scale, transform.localScale.y, scale);
            }
            else
            {
                transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y, scale);
            }

            // set location
            do
            {
                transform.position = new Vector3(Random.Range(minX, maxX), transform.position.y, Random.Range(minZ, maxZ));
            }
            while (Physics.OverlapSphere(transform.position, 2f, 2).Length >= 1);
        }
    }
}
