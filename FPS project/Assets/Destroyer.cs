using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Destroyer : MonoBehaviour
{
    public float TimeToDestroy;
    private void Start()
    {
        Invoke(nameof(Disable), TimeToDestroy);
    }
    void Disable()
    {
        Destroy(gameObject);
    }
}
