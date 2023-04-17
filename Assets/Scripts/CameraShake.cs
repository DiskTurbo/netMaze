using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraShake : MonoBehaviour
{

    public IEnumerator Shake(float duration, float magnitude)
    {
        float timePassed = 0;

        while(timePassed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            transform.localPosition = new Vector3(x, y, 0);

            timePassed += Time.deltaTime;

            yield return null;
        }

        transform.localPosition = Vector3.zero;
    }
}
