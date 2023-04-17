using UnityEngine;
using UnityEngine.UI;

using TMPro;

public class RemoteEffects : WeaponEffects_Base
{
    public TextMeshProUGUI textToChange;
    private GunSystem GS;

    private void OnEnable()
    {
        GS = gameObject.GetComponent<GunSystem>();
        UpdateTxt();
    }

    public override void OnShoot()
    {

    }

    public override void OnReload()
    {

    }

    private void Update()
    {
        UpdateTxt();
    }

    void UpdateTxt()
    {
        textToChange.text = GS.bulletsLeft.ToString();
        textToChange.color = Color.Lerp(Color.red, Color.green, (float)GS.bulletsLeft / (float)GS.magazineSize);
    }
}