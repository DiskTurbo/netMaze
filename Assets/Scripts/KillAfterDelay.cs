using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillAfterDelay : MonoBehaviour
{
    int Delay = 5;

    private void Start()
    {
        Destroy(gameObject, Delay);
    }
}
