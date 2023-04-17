using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

public class Hitmarker : MonoBehaviour
{
    int HMstack;
    int HMstack_Post;

    RawImage Marker;

    private void Awake()
    {
        Marker = GetComponent<RawImage>();
        Marker.enabled = false;
    }

    private void Update()
    {
        if (HMstack != HMstack_Post && HMstack == 0)
            Marker.enabled = false;

        HMstack_Post = HMstack;
    }

    public void CharHit()
    {
        StartCoroutine(nameof(StackPass));

        //
        // Hitmarker Audio Pass - NULL
        //
    }

    IEnumerator StackPass()
    {
        Marker.enabled = true;

        HMstack++;
        yield return new WaitForSeconds(.05f);
        HMstack--;
    }
}
