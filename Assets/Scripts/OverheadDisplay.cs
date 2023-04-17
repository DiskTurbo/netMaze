using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OverheadDisplay : MonoBehaviour
{
    public GameObject Healthbar;

    float upTime = 0f;
    bool isOn = false;

    private void Awake()
    {
        IsLost();
    }

    public void IsSeen()
    {
        upTime = 1f;
        isOn = true;

        Healthbar.SetActive(true);
    }

    private void IsLost()
    {
        isOn = false;

        Healthbar.SetActive(false);
    }

    private void Update()
    {
        if (upTime > 0)
        {
            upTime -= Time.deltaTime;
        }
        else if(upTime <= 0 && isOn)
        {
            IsLost();
        }
    }
}
