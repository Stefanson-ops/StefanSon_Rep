using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Combat_Controller : MonoBehaviour
{
    [Header("Shooting")]
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
    [Header("Fatality")]
    public bool InAction;
    public Transform Camera;
    public LayerMask WhatIsEnemy;
    public float FatalityDistanse;
    public KeyCode FatalityKey;

    [Header("Grappling")]
    public float GrabForce;
    public float GrabReloadTime;
    public bool CanGrab;
    RaycastHit GrabPointHitInfo;
    public float GrabDistanse;
    public Transform GrabPoint;

    [Header("Components")]
    Character_Controller Controller;
    Camera_Controller CameraController;
    public Enemy_Stats Estats;
    RaycastHit Hit;

    private void Start()
    {
        Controller = GetComponent<Character_Controller>();
        CameraController = GetComponentInChildren<Camera_Controller>();
        InAction = false;
        CanGrab = true;
        CanTakeGun = true;
    }
    private void Update()
    {
        CheckForGrappling();
        CheckForEnemy();
        MyInput();
        if (HaveGun)
            MoveGunToHand();
        Physics.Raycast(Camera.position, Camera.forward, out EnemyHit, 1000);
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
                if (Physics.SphereCast(Camera.position - transform.forward / 2, 1, Camera.forward, out hit, FatalityDistanse) && hit.collider.CompareTag("Enemy"))
                {
                    Estats = Hit.collider.GetComponentInParent<Enemy_Stats>();
                    print("Enemy");
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
            if (Physics.SphereCast(Camera.position - transform.forward / 2, 1, Camera.forward, out GrabPointHitInfo, GrabDistanse))
            {
                if (Physics.SphereCast(Camera.position - transform.forward / 2, 1, Camera.forward, out hit, GrabDistanse) && hit.collider.CompareTag("Grab Point"))
                {
                    GrabPoint = GrabPointHitInfo.collider.GetComponent<Transform>();
                }
            }
            else
                GrabPoint = null;
        }
    }
    void ReloadGrappling()
    {
        CanGrab = true;
    }
    #endregion
    void MyInput()
    {
        if (Input.GetKeyDown(FatalityKey) && Estats != null)
        {
            if (Estats.IsStanned && !InAction && Hit.collider.GetComponentInParent<Enemy_Controller>().StillAlive)
            {
                InAction = true;
                Controller.CanMove = false;
                CameraController.CanLook = false;
                Controller.rb.isKinematic = true;
                StartCoroutine(FatalityDuration());
                Hit.collider.GetComponentInParent<Enemy_Controller>().InGrab();
            }
        }
        if (Input.GetKeyDown(KeyCode.E) && GrabPoint != null)
        {
            CanGrab = false;
            Controller.Grappling(GrabPoint, GrabForce);
            Invoke(nameof(ReloadGrappling), GrabReloadTime);
        }
        if (Input.GetKeyDown(TakeGunKey)&& CanTakeGun)
        {
            if (!HaveGun)
                CheckForGun();
            else
                ThrowGun();
        }
    }
    void CheckForGun()
    {
        //if (Physics.Raycast(Camera.position, Camera.forward, out GunHit, TakeGunDistanse, WhatIsGun))
        if (Physics.SphereCast(Camera.position - transform.forward / 2, 1, Camera.forward, out GunHit, TakeGunDistanse, WhatIsGun))
        {
            if (Physics.Raycast(Camera.position, GunHit.collider.gameObject.transform.position - Camera.position, out GunHit, TakeGunDistanse))
            {
                if (GunHit.collider.CompareTag("Gun"))
                {
                    CurrentGun = GunHit.collider.gameObject;
                    CurrentGun.GetComponent<Rigidbody>().isKinematic = true;
                    Invoke(nameof(GetGun), 0.5f);
                    HaveGun = true;

                }
            }
        }
    }
    void MoveGunToHand()
    {
        CurrentGun.transform.position = Vector3.Lerp(CurrentGun.transform.position, GunPosition.position, GunSpeed * Time.deltaTime);
        CurrentGun.transform.rotation = Quaternion.RotateTowards(CurrentGun.transform.rotation, GunPosition.rotation, GunSpeed*10 * Time.deltaTime);
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
        if(CurrentGun!=null)
            CurrentGun.transform.SetParent(GunPosition);
    }
    void ReloadCanTakeGun()
    {
        CanTakeGun = true;
    }
}
