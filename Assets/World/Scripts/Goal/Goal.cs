using UnityEngine;

public class Goal : MonoBehaviour
{
    public float positionRange;
    public float minScale, maxScale;
    [HideInInspector] public HaulerAgent agent;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("target"))
        {
            agent.MarkTaskDone(TaskEndReason.Finished);
        }
    }

    public void Reset(Vector3 targetLocation)
    {
        float scale = Random.Range(minScale, maxScale);
        Vector3 newPos;
        do
        {
            newPos = new Vector3(Random.Range(-1f, 1f) * positionRange, transform.localPosition.y, Random.Range(-1f, 1f) * positionRange);
        }
        while (Vector3.Distance(newPos, targetLocation) < 5 || Physics.OverlapSphere(transform.position, 2f, 2).Length >= 1);

        transform.localPosition = newPos;
        transform.localScale = new Vector3(scale, transform.localScale.y, scale);
    }
}
