using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet_Controller : MonoBehaviour
{
    public float LifeTime;
    public float Dmg;

    private void Start()
    {
        Invoke(nameof(Destroying), LifeTime);
    }
    void Destroying()
    {
        Destroy(gameObject);
    }
    private void OnCollisionEnter(Collision collision)
    {
        foreach(Collider col in Physics.OverlapSphere(transform.position, 10))
        {
            Rigidbody rb = col.GetComponentInChildren<Rigidbody>();
            if (rb != null)
                rb.AddExplosionForce(1000, transform.position, 10);
        }
        if (collision.collider.GetComponentInParent<Enemy_Stats>())
        {
            print("Gotcha");
            collision.collider.GetComponentInParent<Enemy_Stats>().CurrentHP -= Dmg;
        }
        Invoke(nameof(Destroying), 0.01f);
    }
}
