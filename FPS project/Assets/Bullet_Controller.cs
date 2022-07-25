using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet_Controller : MonoBehaviour
{
    public GameObject Part;
    public LayerMask WhatIsHittable;
    public float LifeTime;
    public float Dmg;
    Vector3 PrevPosition;
    RaycastHit hit;

    private void Start()
    {
        Invoke(nameof(Destroying), LifeTime);
    }
    private void Update()
    {
        if (Physics.Raycast(transform.position - transform.forward, transform.forward, out hit, 1, WhatIsHittable))
        {
            Instantiate(Part, hit.point, Quaternion.identity);
            foreach (Collider col in Physics.OverlapSphere(hit.point, 10))
            {
                Rigidbody rb = col.GetComponentInChildren<Rigidbody>();
                if (rb != null)
                    rb.AddExplosionForce(1000, transform.position, 10);
            }

            if (hit.collider.GetComponentInParent<Enemy_Stats>())
            {
                print("Gotcha");
                hit.collider.GetComponentInParent<Enemy_Stats>().CurrentHP -= Dmg;
            }
            Destroy(gameObject);
        }
    }
    void Destroying()
    {
        Destroy(gameObject);
    }
}
