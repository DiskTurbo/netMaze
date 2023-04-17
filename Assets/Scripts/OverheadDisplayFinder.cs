using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OverheadDisplayFinder : MonoBehaviour
{
    Camera cam;

    private void Start()
    {
        cam = GetComponent<Camera>();
    }

    private void Update()
    {
        Ray ray = cam.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f, 0f));
        RaycastHit hit;

        if(Physics.Raycast(ray, out hit, Mathf.Infinity))
        {
            hit.collider.GetComponentInParent<OverheadDisplay>()?.IsSeen();
        }
    }
}
