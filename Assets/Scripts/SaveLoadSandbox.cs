using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;
using Photon.Pun;

public class SaveLoadSandbox : MonoBehaviour
{
    private static char slash = Path.DirectorySeparatorChar;
    public static SaveLoadSandbox instance;
    public static string folderPath = slash + "CustomMaps" + slash;

    private GameObject mapsObject;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
    }
    // Start is called before the first frame update
    void Start()
    {
        mapsObject = GameObject.Find("Maps");
        if(!Directory.Exists(Application.dataPath+folderPath))
        {
            Directory.CreateDirectory(Application.dataPath + folderPath);
        }
    }

    // Update is called once per frame
    void Update()
    {
        /*if(Input.GetKeyDown(KeyCode.LeftBracket))
        {
            //todo: popup filename box

            if(SaveBlocks("testMap.json"))
            {
                //todo: say file was duplicate name or whatever
            }
        }*/

        /*if(Input.GetKeyDown(KeyCode.RightBracket))
        {
            LoadBlocks("testMap.json");
        }*/
    }

    public bool SaveBlocks(string fileName)
    {
        CustomMapData newMap = new CustomMapData();

        for(int i = 0; i<mapsObject.transform.childCount; i++)
        {
            if(mapsObject.transform.GetChild(i).gameObject.activeInHierarchy)
            {
                newMap.mapID = mapsObject.transform.GetChild(i).gameObject.name;
                break;
            }
        }

        GameObject[] blockObjects = GameObject.FindGameObjectsWithTag("Block");

        foreach(GameObject blockObject in blockObjects)
        {
            newMap.positions.Add(blockObject.transform.position);
            newMap.blockTypes.Add(blockObject.name.Remove(blockObject.name.Length-7));
        }


       string json = JsonUtility.ToJson(newMap,true);

        if(File.Exists(Application.dataPath+fileName))
        {
            //return false;
        }

        StreamWriter sw = new StreamWriter(Application.dataPath+folderPath+fileName,false,Encoding.UTF8);

        sw.Write(json);

        sw.Close();

        //Application.OpenURL("file:\\\\" + Application.dataPath);

        return true;

    }

    public bool LoadBlocks(string fileName)
    {
        if(!File.Exists(Application.dataPath+folderPath+fileName))
        {
            return false;
        }
        GameObject[] blockObjects = GameObject.FindGameObjectsWithTag("Block");

        foreach (GameObject blockObject in blockObjects)
        {
            PhotonNetwork.Destroy(blockObject);
        }


        StreamReader sr = new StreamReader(Application.dataPath + folderPath+fileName,Encoding.UTF8);
        string json = sr.ReadToEnd();
        sr.Close();

        CustomMapData loadMap = JsonUtility.FromJson<CustomMapData>(json);

        for(int i = 0; i<loadMap.positions.Count;i++)
        {
            PhotonNetwork.Instantiate(Path.Combine("Blocks", loadMap.blockTypes[i]), loadMap.positions[i], Quaternion.identity);
        }

        return true;
    }
}

public class CustomMapData
{
    public string mapID;
    public string version;
    public List<Vector3> positions;
    public List<string> blockTypes;

    public CustomMapData()
    {
        positions = new List<Vector3>();
        blockTypes = new List<string>();
        
        version = Application.version;
    }
}