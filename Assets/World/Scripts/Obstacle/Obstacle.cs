using UnityEngine;

public class Obstacle : MonoBehaviour
{
    public float maxX, minX, maxZ, minZ;
    private float posY;
    private int layerMask;

    private void Start()
    {
        posY = gameObject.transform.position.y;
        layerMask = 2;
        //layerMask = ~layerMask;
    }

    public void SetActive(bool value)
    {
        gameObject.SetActive(value);
    }
    
    public virtual void Reset()
    {
        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
        }

        if (maxX + maxZ + minX + minZ != 0)
        {
            // reset location
            float x = Random.Range(minX, maxX);
            float z = Random.Range(minZ, maxZ);

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
