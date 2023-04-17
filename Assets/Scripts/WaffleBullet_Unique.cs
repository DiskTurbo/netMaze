using UnityEngine;

public class WaffleBullet_Unique : MonoBehaviour
{
    public GameObject waffle;
    public float torqueMax;
    public float forceMax;
    private void OnDestroy()
    {
        GameObject m_waffleWorld = Instantiate(waffle, transform.position, Quaternion.identity);

        Rigidbody m_rb = m_waffleWorld.GetComponent<Rigidbody>();
        m_rb.AddTorque(new Vector3(Random.Range(-torqueMax, torqueMax), Random.Range(-torqueMax, torqueMax), Random.Range(-torqueMax, torqueMax)));
        m_rb.AddForce(new Vector3(Random.Range(-forceMax, forceMax), Random.Range(-forceMax, forceMax), Random.Range(-forceMax, forceMax)), ForceMode.Impulse);
    }
}