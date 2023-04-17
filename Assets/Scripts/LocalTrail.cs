using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class LocalTrail : MonoBehaviour
{
    [HideInInspector] public Vector3 start;
    [HideInInspector] public Vector3 end;

    public float speed = 250f;

    [HideInInspector] public Vector3 networkedStart;

    private TrailRenderer trail;

    private void Awake()
    {
        trail = GetComponentInChildren<TrailRenderer>();
        Destroy(gameObject, 1f);
    }

    public void SetInfo(Vector3 s, Vector3 e) {
        start = s;
        end = e;

        transform.position = s;

        transform.LookAt(end);
    }

    private void Update()
    {
        transform.position = Vector3.MoveTowards(transform.position, end, Time.deltaTime * speed);
    }

    private void OnTriggerEnter(Collider other)
    {
        Destroy(gameObject);
    }

    void OnDestroy()
    {
        trail.transform.parent = null;
        trail.autodestruct = true;
    }

}
