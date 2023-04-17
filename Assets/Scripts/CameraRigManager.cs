using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraRigManager : MonoBehaviour
{
    public List<Camera> cameras;

    public PlayerMovement PM;

    private void Start()
    {
        ChangeCamera(0);
    }

    public void ChangeCamera(int index)
    {
        KillAll();
        if (cameras[index] != null)
        {
            cameras[index].tag = "MainCamera";
            cameras[index].gameObject.SetActive(true);
        }
    }

    private void KillAll()
    {
        foreach (Camera c in cameras)
        {
            if (c != null)
            {
                if (c.tag == "KillerCam" && PM.isDead)
                    return;

                c.tag = "Untagged";
                c.gameObject.SetActive(false);
            }
        }
    }

    public Camera ActivateKillerCamera()
    {
        cameras[2].gameObject.SetActive(true);
        return cameras[2];
    }

    public void RandomDeathCamera()
    {
        int t = Random.Range(3, cameras.Count);
        Debug.Log(t);
        ChangeCamera(t);
    }
}
