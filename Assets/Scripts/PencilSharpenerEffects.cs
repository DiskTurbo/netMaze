using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PencilSharpenerEffects : WeaponEffects_Base
{
    public Transform spinBase;

    private float targetRotationSpeed;
    private float rotationAcceleration = Mathf.Infinity;
    private float rotationSpeed;

    public GameObject PencilCase;
    MeshRenderer[] MR;
    bool[] isGone;

    int count = 0;
    int goal = 4;

    private void Start()
    {
        MR = PencilCase.GetComponentsInChildren<MeshRenderer>();
        isGone = new bool[MR.Length];
        for (int i = 1; i < isGone.Length; i++)
        {
            isGone[i] = false;
        }
    }

    void ResetAmmo()
    {
        for(int i = 1; i < MR.Length; i++)
        {
            MR[i].enabled = true;
            isGone[i] = false;
        }
    }

    public override void OnShoot()
    {
        targetRotationSpeed = 500f;

        count++;
        if(count == goal)
        {
            count = 0;
            int r = Random.Range(1, MR.Length);
            do
            {
                r = Random.Range(1, MR.Length);
            } while (isGone[r] == true);
            MR[r].enabled = false;
            isGone[r] = true;
        }
    }

    public override void OnReload()
    {
        ResetAmmo();
    }

    private void Update()
    {
        if (targetRotationSpeed > 0)
            targetRotationSpeed-=5;

        rotationSpeed = Mathf.MoveTowards(rotationSpeed, targetRotationSpeed, rotationAcceleration * Time.deltaTime);
        rotationSpeed = Mathf.Clamp(rotationSpeed, 0, 360);

        spinBase.transform.Rotate(0, 0, rotationSpeed * Time.deltaTime, Space.Self);
    }
}
