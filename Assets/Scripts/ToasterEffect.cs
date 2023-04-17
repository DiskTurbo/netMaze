using UnityEngine;

public class ToasterEffect : WeaponEffects_Base
{
    public Transform lever;
    float startY = 0.9f;
    float endY = -0.9f;

    private void Start()
    {

    }

    public override void OnShoot()
    {
        lever.localPosition = new Vector3(
            lever.localPosition.x,
            endY,
            lever.localPosition.z);
    }

    public override void OnReload()
    {

    }

    private void Update()
    {
        if (lever.localPosition.y < startY)
        {
            lever.localPosition = new Vector3(
            lever.localPosition.x,
            lever.localPosition.y + 2.5f * Time.deltaTime,
            lever.localPosition.z);
        }
    }
}