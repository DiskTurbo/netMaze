using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using JSAM;
using Photon.Pun;

public class MeleeSystem : MonoBehaviour
{
    public int damage;
    public float attackSpeed;

    public float timeBetweenAttacks = 1.4f;

    public float knockbackPower;
    public float upForce;

    public Vector3 startPos;
    public Vector3 startRot;
    [Space(25)]
    public Vector3 endPos;
    public Vector3 endRot;

    Camera playerCam;

    PhotonView PV;

    [HideInInspector] public PlayerManager playerManager;
    [HideInInspector] public bool isAttacking = false;
    [HideInInspector] public PlayerMovement pm;

    private void Awake()
    {
        PV = GetComponentInParent<PhotonView>();

        playerCam = Camera.main;
    }

    public IEnumerator Attack()
    {
        float t = 0;
        isAttacking = true;
        JSAM.AudioManager.PlaySound(Sounds.meleeslash, this.transform);
        while (isAttacking)
        {
            transform.localPosition = Vector3.Lerp(startPos, endPos, t);
            transform.localRotation = Quaternion.Slerp(Quaternion.Euler(startRot), Quaternion.Euler(endRot), t);

            t += Time.deltaTime * attackSpeed;

            if (t >= 1)
                isAttacking = false;

            yield return null;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Vector3 forceDir = -(playerCam.transform.position - other.transform.position);

        other.gameObject.GetComponentInParent<PlayerMovement>()?.ApplyForce(forceDir.normalized, knockbackPower, upForce);

        if (other.GetComponentInParent<PhotonView>() != null && !other.gameObject.GetComponentInParent<PhotonView>().IsMine)
            other.gameObject.GetComponentInParent<IDamageable>()?.TakeDamage(damage);

    }
}
