using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Photon.Pun;

public class DeathMessage : MonoBehaviour
{
    public TMP_Text killer;
    public TMP_Text victim;
    public Image weapon;
    public Texture2D skull;
    public Transform deathBoard;

    public void Initialize(string _killer, string _victim)
    {
        deathBoard = GameObject.Find("Deathboard").transform;
        this.transform.SetParent(deathBoard);
        this.transform.localScale = new Vector3(1, 1, 1);
        if (deathBoard.childCount > 3)
        {
            Destroy(deathBoard.GetChild(0).gameObject);
        }
        if(_killer == _victim)
        {
            killer.text = _killer;
            Destroy(weapon.gameObject);
            victim.text = "Self-Elimination!";
        }
        else
        {
            killer.text = _killer;
            victim.text = _victim;
        }
        Destroy(this.gameObject, 15f);
    }
}
