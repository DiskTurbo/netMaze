using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Cinemachine;

public class RandRotCamera : MonoBehaviour
{
    CinemachineFreeLook CFL;

    private void Start()
    {
        CFL = GetComponent<CinemachineFreeLook>();
        CFL.m_Heading.m_Bias = Random.Range(-180f, 180f);
    }

    private void Update()
    {
        CFL.m_Heading.m_Bias += 10f * Time.deltaTime;
    }
}
