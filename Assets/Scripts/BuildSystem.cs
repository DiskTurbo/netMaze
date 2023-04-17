using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Pun;
using System.IO;

public class BuildSystem : MonoBehaviour
{
    public Block[] availableBuildingBlocks;
    int currentBlockIndex = 0;

    Block currentBlock;
    public TMP_Text blockNameText;

    ///public Transform shootingPoint;
    GameObject blockObject;

    public Transform parent;

    public Color normalColor;
    public Color highlightedColor;

    GameObject lastHightlightedBlock;

    PhotonView PV;

    public GameObject buildModeUI;

    public bool isBuilding = false;

    private void Start()
    {
        if (PhotonNetwork.LocalPlayer.CustomProperties["GameMode"].ToString() != "Sandbox")
        {
            this.enabled = false;
        }

        PV = GetComponentInParent<PhotonView>();

        //SetText();
    }

    private void Update()
    {
        if(!PV.IsMine)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.T))
        {
            isBuilding = !isBuilding;
            buildModeUI.SetActive(!buildModeUI.activeInHierarchy);
        }
        if (isBuilding == false)
        {
            return;
        }
        if (Input.GetMouseButtonDown(0))
        {
            BuildBlock(currentBlock.blockObject.name);
        }
        if (Input.GetMouseButtonDown(1))
        {
            DestroyBlock();
        }
        //HighlightBlock();
        ChangeCurrentBlock();
    }

    void ChangeCurrentBlock()
    {
        float scroll = Input.mouseScrollDelta.y;
        if (scroll > 0)
        {
            currentBlockIndex++;
            if (currentBlockIndex > availableBuildingBlocks.Length - 1)
            {
                currentBlockIndex = 0;
            }
        }
        else if (scroll < 0)
        {
            currentBlockIndex--;
            if (currentBlockIndex < 0)
            {
                currentBlockIndex = availableBuildingBlocks.Length - 1;
            }
        }
        //Debug.Log(currentBlockIndex);
        currentBlock = availableBuildingBlocks[currentBlockIndex];
        SetText();
    }

    void SetText()
    {
        blockNameText.text = "Current Block: " + currentBlock.blockName;
    }


    void BuildBlock(string blockname)
    {
        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out RaycastHit hitInfo))
        {

            if (hitInfo.transform.tag == "Block")
            {
                Vector3 spawnPosition = new Vector3(Mathf.RoundToInt(hitInfo.point.x + hitInfo.normal.x / 2), Mathf.RoundToInt(hitInfo.point.y + hitInfo.normal.y / 2), Mathf.RoundToInt(hitInfo.point.z + hitInfo.normal.z / 2));
                //Instantiate(block, spawnPosition, Quaternion.identity, parent);
                PhotonNetwork.Instantiate(Path.Combine("Blocks", blockname), spawnPosition, Quaternion.identity);

            }
            else
            {
                Vector3 spawnPosition = new Vector3(Mathf.RoundToInt(hitInfo.point.x), Mathf.RoundToInt(hitInfo.point.y), Mathf.RoundToInt(hitInfo.point.z));
                PhotonNetwork.Instantiate(Path.Combine("Blocks", blockname), spawnPosition, Quaternion.identity);
            }
        }
    }

    void DestroyBlock()
    {
        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out RaycastHit hitInfo))
        {
            if (hitInfo.transform.tag == "Block")
            {
                if(!hitInfo.transform.gameObject.GetComponentInChildren<PhotonView>().IsMine)
                {
                    hitInfo.transform.gameObject.GetComponentInChildren<PhotonView>().RequestOwnership();
                }
                PhotonNetwork.Destroy(hitInfo.transform.gameObject);
            }
        }
    }

    void HighlightBlock()
    {
        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out RaycastHit hitInfo))
        {
            if (hitInfo.transform.tag == "Block")
            {
                if (lastHightlightedBlock == null)
                {
                    lastHightlightedBlock = hitInfo.transform.gameObject;
                    hitInfo.transform.gameObject.GetComponent<Renderer>().material.color = highlightedColor;
                }
                else if (lastHightlightedBlock != hitInfo.transform.gameObject)
                {
                    lastHightlightedBlock.GetComponent<Renderer>().material.color = normalColor;
                    hitInfo.transform.gameObject.GetComponent<Renderer>().material.color = highlightedColor;
                    lastHightlightedBlock = hitInfo.transform.gameObject;
                }
            }
            else if (lastHightlightedBlock != null)
            {
                lastHightlightedBlock.GetComponent<Renderer>().material.color = normalColor;
                lastHightlightedBlock = null;
            }
        }
    }
}