using Photon.Realtime;
using System.Collections;
using Photon.Pun;
using TMPro;
public class NetPowerUIUpdate : MonoBehaviourPunCallbacks
{
    public TMP_Text scoreText;
    private void Start()
    {
        updateText();
    }
    public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        if (targetPlayer == PhotonNetwork.LocalPlayer)
        {
            if (changedProps.ContainsKey("kills"))
            {
                updateText();
            }
        }
    }
    void updateText()
    {
        if (PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue(("kills"), out object kills))
        {
            scoreText.text = kills.ToString();
        }
    }
}
