using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPreviewScript : MonoBehaviour
{
    public SkinnedMeshRenderer playerModel;
    public MeshRenderer head;
    public Material[] playerColors = new Material[12];
    // Update is called once per frame
    void Update()
    {
        playerModel.material = playerColors[PlayerPrefs.GetInt("PlayerColor")];
        head.material = playerColors[PlayerPrefs.GetInt("PlayerColor")];
        playerModel.GetComponentInParent<Animator>().SetLayerWeight(1, 0);
    }
}
