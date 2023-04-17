using UnityEngine;

public class Teleporter : MonoBehaviour
{
    public Teleporter newPoint;

    public Transform TPPoint;

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponentInParent<PlayerMovement>())
        {
            GameObject go = other.GetComponentInParent<PlayerMovement>()?.gameObject;
            go.transform.position = newPoint.TPPoint.position;
            return;
        }
        else if(other.CompareTag("Interactable"))
        {
            other.transform.position = newPoint.TPPoint.position;
        }
    }
}