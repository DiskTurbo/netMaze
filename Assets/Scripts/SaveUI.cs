using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class SaveUI : MonoBehaviour
{
    public GameObject savePanel;
    public GameObject overwritePanel;
    public TMP_InputField fileName;
    public GameObject feedbackText;

    private float remainTimer = 0.0f;

    public bool inSaveMenu = false;

    private void Awake()
    {

    }

    // Start is called before the first frame update
    void Start()
    {
        overwritePanel.SetActive(false);
        savePanel.SetActive(false);
        feedbackText.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if(remainTimer > 0.0f)
        {
            remainTimer-=Time.deltaTime;
            if(remainTimer < 0.0f)
            {
                feedbackText.SetActive(false);
            }
        }
        /*if(Input.GetKeyDown(KeyCode.LeftBracket))
        {
            inSaveMenu=!inSaveMenu;

            if(inSaveMenu)
            {
                savePanel.SetActive(true);
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                savePanel.SetActive(false);
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }*/
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (inSaveMenu)
            {
                inSaveMenu = false;
                savePanel.SetActive(false);
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }

        
    }

    public void PauseSave()
    {
        inSaveMenu = true;

        savePanel.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void SaveButton()
    {
        if (!fileName.text.EndsWith(".json"))
        {
            fileName.text = fileName.text + ".json";
        }

        if (File.Exists(Application.dataPath +SaveLoadSandbox.folderPath+ fileName.text))
        {
            overwritePanel.SetActive(true);
            return;
        }
        ReallySave();
    }

    public void ReallySave()
    {
        if (!fileName.text.EndsWith(".json"))
        {
            fileName.text = fileName.text + ".json";
        }

        overwritePanel.SetActive(false);
        SaveLoadSandbox.instance.SaveBlocks(fileName.text);

        feedbackText.SetActive(true);
        remainTimer += 5.0f;
    }

    public void TextChanged()
    {
        overwritePanel.SetActive(false);
    }

}
