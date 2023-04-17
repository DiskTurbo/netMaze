using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

using Photon.Pun;

public class Inventory : MonoBehaviour
{
    PhotonView PV;
    [HideInInspector] public PlayerManager playerManager;

    public Color Selected;
    public Color Unselected;

    public bool isPrimary;
    public bool isSecondary;
    public bool isMelee;

    RawImage[] Borders;

    private void Start()
    {
        PV = GetComponentInParent<PhotonView>();
        if (!PV.IsMine)
            return;

        playerManager = PhotonView.Find((int)PV.InstantiationData[0]).GetComponent<PlayerManager>();

        Borders = GetComponentsInChildren<RawImage>();

        for(int i = 0; i < Borders.Length; i++){
            Borders[i].color = Unselected;
        }
        if(isPrimary)
            Borders[playerManager.primaryIndex].color = Selected;
        else if (isSecondary)
            Borders[playerManager.secondaryIndex].color = Selected;
        else if (isMelee)
            Borders[playerManager.meleeIndex].color = Selected;
    }

    public void NewPrimary(GameObject primary)
    {
        if (!PV.IsMine)
            return;

        playerManager.Primary = primary;
    }
    public void NewSecondary(GameObject secondary)
    {
        if (!PV.IsMine)
            return;

        playerManager.Secondary = secondary;
    }
    public void NewMelee(GameObject melee)
    {
        if (!PV.IsMine)
            return;

        playerManager.Melee = melee;
    }

    public void SetSelected(RawImage background)
    {
        if (!PV.IsMine)
            return;

        for (int i = 0; i < Borders.Length; i++)
        {
            Borders[i].color = Unselected;
        }
        background.color = Selected;
    }

    public void SetIndex(int index)
    {
        if (!PV.IsMine)
            return;

        if (isPrimary)
            playerManager.primaryIndex = index;
        if (isSecondary)
            playerManager.secondaryIndex = index;
        if (isMelee)
            playerManager.meleeIndex = index;
    }
}
