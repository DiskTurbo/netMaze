using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

using TMPro;

public class GunHolderManager : MonoBehaviour
{
    private GameObject currentGun = null;

    public GameObject Gun_1, Gun_2;

    public GameObject Melee;

    public TextMeshProUGUI currentAmmoText;
    public TextMeshProUGUI totalAmmoText;

    private MeleeSystem meleeSystem;
    private float timeBetweenMeleeAttacks = 0f, timeSinceMeleeAttack = 0f;
    private bool canAttackMelee;

    [Space(40)]
    public Image slider;
    public GameObject meleeReloadCanv;

    bool onGun1;

    public void SetWeaponsStart()
    {
        onGun1 = true;

        meleeSystem = Melee.GetComponent<MeleeSystem>();
        timeBetweenMeleeAttacks = meleeSystem.timeBetweenAttacks;

        Gun_1.SetActive(true);
        currentGun = Gun_1;

        Gun_2.SetActive(false);
        Melee.SetActive(false);
    }

    private void Update()
    {
        currentAmmoText.text = 
            (currentGun.GetComponent<GunSystem>().bulletsLeft / currentGun.GetComponent<GunSystem>().bulletsPerTap).ToString();
        totalAmmoText.text = 
            (currentGun.GetComponent<GunSystem>().magazineSize / currentGun.GetComponent<GunSystem>().bulletsPerTap).ToString();

        if (Gun_1.gameObject.GetComponent<GunSystem>().pm.PV.IsMine && Gun_1.gameObject.GetComponent<GunSystem>().pm.isEmote || Gun_2.GetComponent<GunSystem>().pm.isEmote && Gun_2.GetComponent<GunSystem>().pm.PV.IsMine || Melee.GetComponent<MeleeSystem>().pm.isEmote && Gun_2.GetComponent<GunSystem>().pm.PV.IsMine)
        {
            return;
        }
        if (Input.GetKeyDown(KeyCode.Z) || Mathf.Abs(Input.GetAxisRaw("Mouse ScrollWheel")) > .1 && !ChatManager.Instance.isChatting)
        {
            StartCoroutine(nameof(WeaponSwap));
        }

        if (timeSinceMeleeAttack < timeBetweenMeleeAttacks) timeSinceMeleeAttack += Time.deltaTime;
        canAttackMelee = timeSinceMeleeAttack > timeBetweenMeleeAttacks ? true : false;

        meleeReloadCanv.SetActive(false);
        if (!canAttackMelee)
        {
            meleeReloadCanv.SetActive(true);
            slider.fillAmount = timeSinceMeleeAttack / timeBetweenMeleeAttacks;
        }

        if (Input.GetKey(KeyCode.V) && canAttackMelee && !ChatManager.Instance.isChatting)
        {
            timeSinceMeleeAttack = 0f;  
            StartCoroutine(nameof(MeleeStrike));
        }
    }

    public IEnumerator WeaponSwap()
    {
        if (onGun1)
        {
            onGun1 = false;
            GunSystem GS_1 = Gun_1.GetComponent<GunSystem>();
            GunSystem GS_2 = Gun_2.GetComponent<GunSystem>();

            StartCoroutine(GS_1.SwapOut());

            while (GS_1.swaping)
                yield return null;

            Gun_1.SetActive(false);
            Gun_2.SetActive(true);
            currentGun = Gun_2;

            StartCoroutine(GS_2.SwapIn());

            while (GS_2.swaping)
                yield return null;
        }
        else 
        {
            onGun1 = true;
            GunSystem GS_1 = Gun_1.GetComponent<GunSystem>();
            GunSystem GS_2 = Gun_2.GetComponent<GunSystem>();

            StartCoroutine(GS_2.SwapOut());

            while (GS_2.swaping)
                yield return null;

            Gun_1.SetActive(true);
            Gun_2.SetActive(false);
            currentGun = Gun_1;

            StartCoroutine(GS_1.SwapIn());

            while (GS_1.swaping)
                yield return null;
        }
    }
    public IEnumerator MeleeStrike()
    {
        MeleeSystem MS = Melee.GetComponent<MeleeSystem>();
        if (onGun1)
        {
            GunSystem GS_1 = Gun_1.GetComponent<GunSystem>();

            StartCoroutine(GS_1.SwapOut());

            while (GS_1.swaping)
                yield return null;

            Gun_1.SetActive(false);
            Melee.SetActive(true);

            StartCoroutine(MS.Attack());

            while (MS.isAttacking)
                yield return null;

            Gun_1.SetActive(true);
            Melee.SetActive(false);

            StartCoroutine(GS_1.SwapIn());
            while (GS_1.swaping)
                yield return null;
        }
        else
        {
            GunSystem GS_2 = Gun_2.GetComponent<GunSystem>();

            StartCoroutine(GS_2.SwapOut());

            while (GS_2.swaping)
                yield return null;

            Gun_2.SetActive(false);
            Melee.SetActive(true);

            StartCoroutine(MS.Attack());

            while (MS.isAttacking)
                yield return null;

            Gun_2.SetActive(true);
            Melee.SetActive(false);

            StartCoroutine(GS_2.SwapIn());
            while (GS_2.swaping)
                yield return null;
        }
    }
}
