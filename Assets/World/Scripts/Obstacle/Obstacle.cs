using UnityEngine;

public class Obstacle : MonoBehaviour
{
    public float maxX, minX, maxZ, minZ, minScale, maxScale;
    private float posY;
    private int layerMask;

    private void Start()
    {
        posY = transform.position.y;
        layerMask = 2;
    }
    
    public virtual void Reset()
    {
        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
        }

        if (maxX + maxZ != 0 || minX + minZ != 0)
        {
            float scale = Random.Range(minScale, maxScale);
            float x = Random.Range(minX, maxX);
            float z = Random.Range(minZ, maxZ);

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
            if (Physics.OverlapSphere(new Vector3(x, posY, z), 4f, layerMask).Length == 1)
            {
                Reset();
            }     
            else
            {
                gameObject.transform.position = new Vector3(x, posY, z);
            }
        }
    }
}
