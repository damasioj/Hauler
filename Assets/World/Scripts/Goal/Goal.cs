using UnityEngine;

public class Goal : MonoBehaviour
{
    public float positionRange;
    public float minScale, maxScale;
    [HideInInspector] public HaulerAgent agent;

    private Vector3 _targetLocation;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("target"))
        {
            agent.MarkTaskDone(TaskEndReason.Finished);
        }
    }

    public void Reset(Vector3 targetLocation)
    {
        _targetLocation = targetLocation;

        float scale = Random.Range(minScale, maxScale);
        Vector3 newPos;
        do
        {
            newPos = new Vector3(Random.Range(-1f, 1f) * positionRange, transform.localPosition.y, Random.Range(-1f, 1f) * positionRange);
        }
        while (newPos.sqrMagnitude - _targetLocation.sqrMagnitude < 5);

        transform.localPosition = newPos;
        transform.localScale = new Vector3(scale, transform.localScale.y, scale);
    }
}
