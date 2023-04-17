using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazeGeneration : MonoBehaviour
{
    [Header("Map Data")]
    public int mapSize;

    [Space(50)]
    [Header("Prefabs")]
    GameObject PREF_Floor;
    GameObject PREF_Wall;
}
