using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_Stats : MonoBehaviour
{
    public Enemy_Controller EController;
    public float MaxHP;
    public float CurrentHP;
    public bool IsStanned;
    public Transform FatalPosition;
    public Transform FatalPosition1;
    public Transform FatalPosition2;
    public Transform LookPoint;
    private void Start()
    {
        EController = GetComponent<Enemy_Controller>();
    }
    private void Update()
    {
        if (CurrentHP < 40)
            IsStanned = true;
        NearFatalPointPosition();
    }
    void NearFatalPointPosition()
    {
        if (Vector3.Distance(EController.Player.position, FatalPosition1.position) < Vector3.Distance(EController.Player.position, FatalPosition2.position))
        {
            FatalPosition = FatalPosition1;
        }
        else
            FatalPosition = FatalPosition2;
    }
}
