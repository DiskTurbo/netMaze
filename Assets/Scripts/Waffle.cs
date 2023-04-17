using UnityEngine;

public class Waffle : MonoBehaviour
{
    Rigidbody _rb;
    MeshCollider _mc;

    float counter;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _mc = GetComponent<MeshCollider>();

        Destroy(this.gameObject, 60f);
    }

    private void Update()
    {
        if(_rb.velocity.magnitude < .1f)
        {
            counter += Time.deltaTime;
        }
        if(counter > .5f)
        {
            Destroy(_rb);
            Destroy(_mc);
            Destroy(this);
        }
    }
}