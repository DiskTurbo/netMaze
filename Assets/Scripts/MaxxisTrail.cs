using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaxxisTrail : MonoBehaviour
{
    TrailRenderer TR;

    private void Awake()
    {
        TR = GetComponentInChildren<TrailRenderer>();
        Color c = Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);

        TR.startColor = c;
        TR.endColor = c;
    }
}
