using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun_Controller : MonoBehaviour
{
    public float Spread;
    public float Damage;
    public float BulletSpeed;
    public float TimeBetweenShots;
    bool CanShoot = true;
    public Transform BulletPoint;
    public GameObject Bullet;
    public Transform FPSCamera;
    Vector3 TargetPoint;
    public void Shoot()
    {
        if (CanShoot)
        {
            RaycastHit hit;
            if (Physics.Raycast(FPSCamera.position, transform.forward, out hit, 10000))
                TargetPoint = hit.point;
            else
                TargetPoint = FPSCamera.forward * 100000;

            Vector3 DirectionWithoutSpread = TargetPoint - BulletPoint.position;
            float x = Random.Range(-Spread, Spread);
            float y = Random.Range(-Spread, Spread);
            Vector3 DirectionWithSpread = DirectionWithoutSpread + new Vector3(x, y, 0);

            Quaternion rot = Quaternion.LookRotation(DirectionWithSpread - BulletPoint.position);

            BulletPoint.rotation = rot;

            GameObject CurrentBullet = Instantiate(Bullet, BulletPoint.position, BulletPoint.rotation);

            CurrentBullet.GetComponent<Bullet_Controller>().Dmg = Damage;
            CurrentBullet.transform.forward = DirectionWithSpread.normalized;
            CurrentBullet.GetComponent<Rigidbody>().AddForce(DirectionWithSpread.normalized * BulletSpeed, ForceMode.Impulse);
            CanShoot = false;
            Invoke(nameof(ReloadShoot), TimeBetweenShots);
        }
    }
    public void ReloadShoot()
    {
        CanShoot = true;
    }
}
