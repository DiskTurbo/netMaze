using UnityEngine;

public class JumpPad : MonoBehaviour
{
    public float power = 25f    ;

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponentInParent<PlayerMovement>())
        {
            Rigidbody rb = other.GetComponentInParent<PlayerMovement>().GetComponent<Rigidbody>();

            rb.velocity = new Vector3(rb.velocity.x, power, rb.velocity.z);

            //rb.AddForce(transform.up * power, ForceMode.Impulse);
        }
        else if (other.CompareTag("Interactable"))
        {
            Rigidbody rb = other.GetComponent<Rigidbody>();
            rb.velocity = new Vector3(rb.velocity.x, power, rb.velocity.z);
        }
    }
}