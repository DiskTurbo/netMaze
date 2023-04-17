using System.IO;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using JSAM;

using Photon.Pun;

public class GunSystem : MonoBehaviour
{
    [Space(15)]
    [Header("Weapon Statistics")]
    public int damage;
    public float timeBetweenShooting, spread, range, reloadTime, timeBetweenShots, swapSpeed = 1f;
    public int magazineSize, bulletsPerTap;
    public bool allowButtonHold;

    [Space(15)]
    [Header("Camera Shake Effects")]
    public float cameraShakeMagnitude;
    public float camShakeDuration;
    CameraShake cameraShake;

    [Space(15)]
    [Header("Effects")]
    public GameObject BulletRenderTrail;
    public GameObject MuzzleFlash;

    [Space(15)]
    [Header("Sound Effects")]
    public Sounds soundEffect;

    [Space(15)]
    [Header("ADS Logics")]
    public float ADSSpeed = 10f;
    public Vector3 ADSPos;

    private bool isADS;

    [Space(15)]
    [Header("Kickback Info")]
    public float kickbackLevel = 1f;

    public PhotonView PV;

    public Transform shootingPoint;
    public RaycastHit hit;

    //bullet & Gun Logic
    [HideInInspector] public int bulletsLeft, bulletsShot;
    bool shooting, readyToShoot, reloading;

    //Camera Data
    Camera playerCam;
    private float camFOVBase;
    private float camFOVADS;

    //Animations
    Animator anim;

    [HideInInspector] public PlayerManager playerManager;
    [HideInInspector] public bool swaping = false;
    [HideInInspector] public PlayerMovement pm;

    WeaponEffects_Base EffectsScript;

    private void Awake()
    {
        PV = GetComponent<PhotonView>();
        EffectsScript = GetComponent<WeaponEffects_Base>();

        //playerManager = PlayerManager.Find(PhotonNetwork.LocalPlayer);

        playerCam = Camera.main;

        camFOVBase = playerCam.fieldOfView;
        camFOVADS = camFOVBase - 20f;
    }

    private void Start()
    {
        bulletsLeft = magazineSize;
        readyToShoot = true;

        cameraShake = GetComponentInParent<CameraShake>();
    }

    private void Update()
    {
        if (swaping)
            return;

        if (pm.isInWeaponSelect)
            return;

        if (pm.isInPauseMenu)
            return;

        if (pm.isEmote)
        {
            //Fix Animation Here
            return;
        }

        if (ChatManager.Instance.isChatting)
            return;

        if (pm.gravGun.enabled && pm.gravGun.isGrabbing)
            return;

        if (pm.buildSys.enabled && pm.buildSys.isBuilding)
            return;


        GetInput();
    }

    void GetInput()
    {
        if (allowButtonHold) shooting = Input.GetKey(KeyCode.Mouse0);
        else shooting = Input.GetKeyDown(KeyCode.Mouse0);

        if (Input.GetKeyDown(KeyCode.R) && bulletsLeft < magazineSize && !reloading)
        {
            if (this.gameObject.GetComponent<Animator>() != null)
                AnimReload();
            else
                Reload();
        }

        if(readyToShoot && shooting && !reloading && bulletsLeft > 0)
        {
            ShootInstance();
        }
        else if(readyToShoot && shooting && !reloading)
        {
            if (this.gameObject.GetComponent<Animator>() != null)
                AnimReload();
            else
                Reload();
        }

        isADS = Input.GetKey(KeyCode.Mouse1);
        if (isADS)
        {
            transform.localPosition = Vector3.Lerp(transform.localPosition, ADSPos, ADSSpeed * Time.deltaTime);
            playerCam.fieldOfView = Mathf.Lerp(playerCam.fieldOfView, camFOVADS, ADSSpeed * Time.deltaTime);
        }
        else
        {
            transform.localPosition = Vector3.Lerp(transform.localPosition, Vector3.zero, ADSSpeed * Time.deltaTime);
            playerCam.fieldOfView = Mathf.Lerp(playerCam.fieldOfView, camFOVBase, ADSSpeed * Time.deltaTime);
        }
    }

    void ShootInstance()
    {
        //
        // Effects that only happen once per shoot instance will happen here (Audio, Animation Etc.)
        // Effects that happen per shot will happen in Shoot() (Bullet Trail, Damage Application Etc.)
        //

        //Muzzle Flash
        GameObject tempMuzzle = Instantiate(MuzzleFlash, shootingPoint.position, shootingPoint.rotation);
        tempMuzzle.transform.Rotate(new Vector3(0, 0, Random.Range(0, 360)), Space.Self);
        tempMuzzle.transform.parent = shootingPoint;
        Destroy(tempMuzzle, .1f);

        bulletsShot = bulletsPerTap;
        transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, transform.localPosition.z - kickbackLevel);
        Shoot();
    }

    void Shoot()
    {
        readyToShoot = false;

        float x = Random.Range(-spread, spread);
        float y = Random.Range(-spread, spread);
        float z = Random.Range(-spread, spread);

        Vector3 direction = playerCam.transform.forward + new Vector3(x, y, z);

        if(EffectsScript != null)
            EffectsScript.OnShoot();

        if (Physics.Raycast(playerCam.transform.position, direction, out hit, range))
        {
            playerManager.gameObject.GetPhotonView().RPC("playShootSound", RpcTarget.All, shootingPoint.position, (byte)soundEffect);

            //Call damage if hit a damageable object or player
            if (hit.collider.gameObject.GetComponentInParent<IDamageable>() != null)
            {
                JSAM.AudioManager.PlaySound(Sounds.hitMark);
                hit.collider.gameObject.GetComponentInParent<IDamageable>().TakeDamage(damage);

                GameObject tempScoreUI = pm.scoreUp.GetTxt();
                if(tempScoreUI != null)
                {
                    tempScoreUI.SetActive(true);
                    tempScoreUI.GetComponent<ScoreUpText>().Run(damage, .5f);
                }

                pm.HM.CharHit();
            }
            //
            /////// Start Physics Pass
            //

            // BROKEN FOR NOW - TRY LATER
            PhysicsManager.instance.ApplyForce(hit.point, 100f, 1.5f);

            //
            /////// End Physics Pass
            //

            //Recoil

            //Bullet Trail
            Vector3 point = hit.point;
            BulletTrail(Path.Combine("WeaponFX", BulletRenderTrail.name), shootingPoint.position, point);
        }

        //
        /////// Start Effects Pass
        //

        //Animation

        //Shake
        StartCoroutine(cameraShake.Shake(camShakeDuration,cameraShakeMagnitude));

        //
        /////// End Effects Pass
        //

        bulletsLeft--;
        bulletsShot--;

        Invoke(nameof(ResetShoot), timeBetweenShooting);

        if(bulletsShot > 0 && bulletsLeft > 0)
            Invoke(nameof(Shoot), timeBetweenShots);
    }

    void ResetShoot()
    {
        readyToShoot = true;
    }

    void Reload()
    {
        StartCoroutine(CallReload());
    }

    IEnumerator CallReload()
    {
        reloading = true;
        /*anim = gameObject.GetComponent<Animator>();
        anim.SetBool("reloading", true);*/

        yield return new WaitForSeconds(reloadTime);

        if (EffectsScript != null)
            EffectsScript.OnReload();

        transform.localRotation = Quaternion.Euler(Vector3.zero);

        bulletsLeft = magazineSize;
        reloading = false;
        //anim.SetBool("reloading", false);
    }

    void AnimReload()
    {
        reloading = true;
        anim = gameObject.GetComponent<Animator>();
        anim.SetBool("reloading", true);
    }

    public void StopAnimReload()
    {
        if (EffectsScript != null)
            EffectsScript.OnReload();

        transform.localRotation = Quaternion.Euler(Vector3.zero);

        bulletsLeft = magazineSize;
        reloading = false;
        anim.SetBool("reloading", false);
    }

    void BulletTrail(string path, Vector3 start, Vector3 end)
    {
        EffectsManager.instance.BulletTrail(path, start, end);
    }


    public IEnumerator SwapOut()
    {
        ResetData();
        float t = 0;
        swaping = true;
        while (swaping)
        {
            transform.localPosition = Vector3.Lerp(Vector3.zero, new Vector3(0, -5, -5), t);
            t += Time.deltaTime * swapSpeed;

            if (t >= 1)
                swaping = false;

            yield return null;
        }
    }
    public IEnumerator SwapIn()
    {
        ResetData();
        float t = 0;
        swaping = true;
        while (swaping)
        {
            transform.localPosition = Vector3.Lerp(new Vector3(0, 5, -5), Vector3.zero, t);
            t += Time.deltaTime * swapSpeed;

            if (t >= 1)
                swaping = false;

            yield return null;
        }
    }

    private void ResetData()
    {
        anim = gameObject.GetComponent<Animator>();
        if(anim != null)
            anim.SetBool("reloading", false);

        playerCam.fieldOfView = camFOVBase;

        transform.localRotation = Quaternion.Euler(Vector3.zero);
        reloading = false;
    }
}
