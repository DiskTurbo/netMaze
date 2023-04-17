using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using JSAM;


public class EmoteMenu : MonoBehaviour
{
    public PhotonView playerPV;

    public Animator uiAnimation;
    public PlayerVisualsManager pvm;

    string animationName;

    void Update()
    {

        if (Input.GetKeyDown(KeyCode.Q))
        {
            this.gameObject.SetActive(false);
        }
        else if (Input.GetKeyDown(KeyCode.G))
        {
            PlayAnim(animationName);
        }
        else if(Input.GetKeyDown(KeyCode.Alpha1))
        {
            PlayAnim("Funnyman Dance");
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            PlayAnim("Funnyman Shuffle");
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            PlayAnim("Funnyman Laugh");
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            PlayAnim("Funnyman Wave");
        }
        else if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            PlayAnim("Hadoken");
        }
    }

    void PlayAnim(string name)
    {
        animationName = name;
        if(animationName != null)
        {
            playerPV.RPC("PlayEmoteAnimation", RpcTarget.AllBuffered, animationName);

        }
        this.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        uiAnimation = GetComponent<Animator>();
        uiAnimation.Play("Emote_onEnable");
    }
}
