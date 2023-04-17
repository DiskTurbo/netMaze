using System;
using System.Collections;

using UnityEngine;

using UnityEngine.UI;

using TMPro;

public class LoadingFact : MonoBehaviour
{
    public TextMeshProUGUI textHold;

    string[] _facts;
    string _path = "Assets/Resources/FunnyFactz.txt";

    private void Start()
    {
        _facts = System.IO.File.ReadAllLines(_path);
    }

    private void OnEnable()
    {
        _facts = System.IO.File.ReadAllLines(_path);
        textHold.text = _facts[UnityEngine.Random.Range(0, _facts.Length)];
    }
}