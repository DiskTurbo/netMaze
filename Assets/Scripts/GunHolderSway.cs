using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunHolderSway : MonoBehaviour
{
    [Header("Position")]
    public float amount = 0.04f;
    public float maxAmount = 0.08f;
    public float smoothAmount = 6f;

    [Header("Rotation")]
    public float rotationAmount = 6f;
    public float maxRotationAmount = 7f;
    public float smoothRotation = 12f;

    [Space]
    public bool rotationX = true;
    public bool rotationY = true;
    public bool rotationZ = true;

    private Vector3 initialPosition;
    private Quaternion initialRotation;

    private float InputX;
    private float InputY;

    PlayerMovement playerMovement;

    private void Start()
    {
        playerMovement = GetComponentInParent<PlayerCam>().playerMovement;

        initialPosition = transform.localPosition;
        initialRotation = transform.localRotation;
    }

    private void Update()
    {
        CalculateSway();
        MoveSway();
        TiltSway();
    }

    void CalculateSway()
    {
        if (playerMovement.isInWeaponSelect)
            return;

        InputX = -Input.GetAxis("Mouse X");
        InputY = -Input.GetAxis("Mouse Y");
    }
    void MoveSway()
    {
        float moveX = Mathf.Clamp(InputX * amount, -maxAmount, maxAmount);
        float moveY = Mathf.Clamp(InputY * amount, -maxAmount, maxAmount);

        Vector3 finalPosition = new Vector3(moveX, moveY, 0);

        transform.localPosition = Vector3.Lerp(transform.localPosition, finalPosition + initialPosition, Time.deltaTime * smoothAmount);
    }
    void TiltSway()
    {
        float tiltY = Mathf.Clamp(InputX * rotationAmount, -maxRotationAmount, maxRotationAmount);
        float tiltX = Mathf.Clamp(InputY * rotationAmount, -maxRotationAmount, maxRotationAmount);

        Quaternion finalRotation = Quaternion.Euler(new Vector3(
            rotationX ? -tiltX : 0f,
            rotationY ? tiltY : 0f,
            0f));

        transform.localRotation = Quaternion.Slerp(transform.localRotation, finalRotation * initialRotation, Time.deltaTime * smoothRotation);
    }
}
