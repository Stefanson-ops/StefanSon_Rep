using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Combat_Controller : MonoBehaviour
{
    [Header("Shooting")]
    public KeyCode ShootKey;
    public float GunSpeed;
    public float GunThrowForce;
    public LayerMask WhatIsGun;
    GameObject CurrentGun;
    public Transform GunPosition;
    public float TakeGunDistanse;
    RaycastHit GunHit;
    RaycastHit EnemyHit;
    bool HaveGun;
    bool CanShoot;
    public KeyCode TakeGunKey;
    bool CanTakeGun;

    [Space]
    [Header("Fatality")]
    public bool InAction;
    public Transform Camera;
    public LayerMask WhatIsEnemy;
    public float FatalityDistanse;
    public KeyCode FatalityKey;

    [Space]
    [Header("Grappling")]
    public float HookSpeed;
    public Transform HookEnd;
    public KeyCode GrabKey;
    public float GrabDistanse;
    public float GrabReloadTime;
    public bool IsHoldingOn;
    public bool CanGrab;
    RaycastHit GrabPointHitInfo;
    public Transform GrabPoint;

    [Space]
    [Header("Components")]
    CableComponent Cable;
    Character_Controller Controller;
    Camera_Controller CameraController;
    public Enemy_Stats Estats;
    RaycastHit Hit;

    private void Start()
    {
        Cable = GetComponent<CableComponent>();
        Controller = GetComponent<Character_Controller>();
        CameraController = GetComponentInChildren<Camera_Controller>();
        InAction = false;
        CanGrab = true;
        CanTakeGun = true;
    }
    private void Update()
    {
        CheckForEnemy();
        MyInput();
        if (HaveGun)
            MoveGunToHand();
        if (HaveGun)
            SetVisible();
    }
    private void FixedUpdate()
    {
        if (InAction)
            Fatality(Estats.LookPoint, Estats.FatalPosition);
    }
    #region Fatality

    void Fatality(Transform LookPoint, Transform FatalPosition)
    {
        CameraController.LookAtPoint(LookPoint);
        Controller.rb.MovePosition(Vector3.Lerp(transform.position, FatalPosition.position, 10 * Time.deltaTime));
    }
    IEnumerator FatalityDuration()
    {
        yield return new WaitForEndOfFrame();
        yield return new WaitForSeconds(Hit.collider.GetComponentInParent<Animator>().GetCurrentAnimatorStateInfo(0).length + 1);
        Controller.CanMove = true;
        Controller.rb.isKinematic = false;
        CameraController.CanLook = true;
        InAction = false;
        CameraController.ResetCameraRotation();
    }
    void CheckForEnemy()
    {
        if (!InAction)
        {
            RaycastHit hit;
            if (Physics.SphereCast(Camera.position - transform.forward / 2, 1, Camera.forward, out Hit, FatalityDistanse, WhatIsEnemy))
            {
                if (Physics.Raycast(Camera.position, Camera.forward, out hit, FatalityDistanse))
                {
                    if (hit.collider.CompareTag("Enemy"))
                    {
                        Estats = Hit.collider.GetComponentInParent<Enemy_Stats>();
                        print("Enemy");
                    }
                }
            }
            else
                Estats = null;
        }
    }
    #endregion
    #region Grappling
    void CheckForGrappling()
    {
        if (!InAction)
        {
            RaycastHit hit;
            if (Physics.SphereCast(Camera.position - transform.forward / 2, 2, Camera.forward, out GrabPointHitInfo, GrabDistanse))
            {
                if (Physics.Raycast(Camera.position + transform.forward, GrabPointHitInfo.point - (Camera.position + transform.forward), out hit, GrabDistanse))
                {
                    if (hit.collider.CompareTag("Hook Point"))
                    {
                        GrabPoint = GrabPointHitInfo.collider.GetComponentInParent<Hook_Point_Controller>().HookPoint;

                        print("!!!");
                    }
                }
            }
            else if (!IsHoldingOn)
                GrabPoint = null;
        }
    }
    void ReloadGrappling()
    {
        CanGrab = true;
    }
    #endregion
    #region Shooting
    void CheckForGun()
    {
        if (Physics.SphereCast(Camera.position - transform.forward / 2, 1, Camera.forward, out GunHit, TakeGunDistanse, WhatIsGun))
        {
            if (Physics.Raycast(Camera.position, GunHit.collider.gameObject.transform.position - Camera.position, out GunHit, TakeGunDistanse))
            {
                if (GunHit.collider.CompareTag("Gun"))
                {
                    CurrentGun = GunHit.collider.gameObject;
                    CurrentGun.GetComponent<Rigidbody>().isKinematic = true;
                    CurrentGun.GetComponent<Gun_Controller>().FPSCamera = CameraController.CameraHolder;
                    Invoke(nameof(GetGun), 0.5f);
                    HaveGun = true;

                }
            }
        }
    }
    void MoveGunToHand()
    {
        CurrentGun.transform.position = Vector3.Lerp(CurrentGun.transform.position, GunPosition.position, GunSpeed * Time.deltaTime);
        CurrentGun.transform.rotation = Quaternion.RotateTowards(CurrentGun.transform.rotation, GunPosition.rotation, GunSpeed * 10 * Time.deltaTime);
    }
    void ThrowGun()
    {
        CanTakeGun = false;
        Invoke(nameof(ReloadCanTakeGun), 1);
        RaycastHit hit;
        CurrentGun.transform.SetParent(null);
        CurrentGun.GetComponent<Rigidbody>().isKinematic = false;
        if (Physics.Raycast(Camera.position, Camera.forward, out hit, 1000) && !Physics.Raycast(Camera.position, Camera.forward, out EnemyHit, 1000, WhatIsEnemy))
        {
            print("Throw");
            CurrentGun.GetComponent<Rigidbody>().AddForce((hit.point - GunPosition.position).normalized * GunThrowForce, ForceMode.Impulse);
        }
        if (Physics.Raycast(Camera.position, Camera.forward, out EnemyHit, 1000, WhatIsEnemy))
        {
            print("ToEnemy");
            CurrentGun.GetComponent<Rigidbody>().AddForce((EnemyHit.point - GunPosition.position).normalized * GunThrowForce, ForceMode.Impulse);
        }
        else
            CurrentGun.GetComponent<Rigidbody>().AddForce(Camera.forward * GunThrowForce, ForceMode.Impulse);
        HaveGun = false;
        CurrentGun = null;
    }
    void GetGun()
    {
        if (CurrentGun != null)
            CurrentGun.transform.SetParent(GunPosition);
    }
    void ReloadCanTakeGun()
    {
        CanTakeGun = true;
    }
    void SetVisible()
    {
        if (InAction)
            CurrentGun.SetActive(false);
        else
            CurrentGun.SetActive(true);
    }
    #endregion
    #region Cable Control
    void CableLenghtControl()
    {
        float lenght;
        if (IsHoldingOn)
            lenght = Vector3.Distance(HookEnd.position, GrabPoint.position) + 5f;
        else
            lenght = 0;
        Cable.cableLength = Mathf.Lerp(Cable.cableLength, lenght, 1);
    }
    void MoveCableToHookPoint()
    {
        HookEnd.position = Vector3.Lerp(HookEnd.position, GrabPoint.position, HookSpeed * Time.deltaTime);
        CableLenghtControl();
    }
    void MoveCableToPlayer()
    {
        HookEnd.position = Vector3.Lerp(HookEnd.position, transform.position - Vector3.up, HookSpeed / 2 * Time.deltaTime);
        CableLenghtControl();
    }
    #endregion
    void MyInput()
    {
        if (Input.GetKeyDown(FatalityKey) && Estats != null)
        {
            if (Estats.IsStanned && !InAction && Estats.EController.StillAlive)
            {
                InAction = true;
                Controller.CanMove = false;
                CameraController.CanLook = false;
                Controller.rb.isKinematic = true;
                StartCoroutine(FatalityDuration());
                Estats.EController.InGrab();
            }
        }
        if (Input.GetKeyDown(GrabKey) && !IsHoldingOn)
        {
            CheckForGrappling();
            if (GrabPoint != null)
            {
                Controller.StartGrapple(GrabPoint);
                IsHoldingOn = true;
                CanGrab = false;
                Invoke(nameof(ReloadGrappling), GrabReloadTime);
            }
        }
        if (IsHoldingOn && Input.GetKeyDown(Controller.JumpButton))
        {
            Controller.StopGrapple();
            IsHoldingOn = false;
        }
        
        if (Input.GetKeyDown(TakeGunKey) && CanTakeGun)
        {
            if (!HaveGun)
                CheckForGun();
            else
                ThrowGun();
        }
        if (Input.GetKey(ShootKey) && HaveGun && !InAction)
        {
            CurrentGun.GetComponent<Gun_Controller>().Shoot();
        }

        if (IsHoldingOn)
            MoveCableToHookPoint();
        else
            MoveCableToPlayer();

    }
}
