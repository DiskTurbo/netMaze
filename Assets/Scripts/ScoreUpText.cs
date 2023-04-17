using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

using TMPro;

public class ScoreUpText : MonoBehaviour
{
    Vector2 moveDir;

    public void Run(int dmg, float duration)
    {
        this.GetComponent<TextMeshProUGUI>().text = "+" + dmg;
        StartCoroutine(nameof(KillAfterDelay), duration);
        transform.localPosition = Vector3.zero;
        moveDir = new Vector2(Random.Range(40f, 120f), Random.Range(20f, 60f));
    }

    private void Update()
    {
        transform.Translate(moveDir * Time.deltaTime, Space.Self);
    }

    private IEnumerator KillAfterDelay(float duration)
    {
        yield return new WaitForSeconds(duration);
        this.gameObject.SetActive(false);
    } 
}
