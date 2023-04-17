using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

using TMPro;

public class ScoreUp : MonoBehaviour
{
    private List<GameObject> pool = new List<GameObject>();
    int poolSize = 20;

    [SerializeField] private GameObject poolItem;

    private void Start()
    {
        for(int i = 0; i < poolSize; i++)
        {
            GameObject txt = Instantiate(poolItem);
            txt.transform.parent = this.transform;
            txt.transform.localPosition = Vector3.zero;
            txt.SetActive(false);
            pool.Add(txt);
        }   
    }

    public GameObject GetTxt()
    {
        for(int i = 0; i < pool.Count; i++)
        {
            if (!pool[i].activeInHierarchy)
                return pool[i];
        }
        return null;
    }
}
