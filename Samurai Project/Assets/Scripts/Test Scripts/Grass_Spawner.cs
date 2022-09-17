using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grass_Spawner : MonoBehaviour
{
    [SerializeField] float Range;
    [SerializeField] GameObject Environment;
    [SerializeField] GameObject[] GrassPrefs;
    [SerializeField] int SmallGrassAmount;
    [SerializeField] int BigGrassAmount;

    [SerializeField] float GrassRotation;
    private void Start()
    {
        for (int i = 0; i < SmallGrassAmount; i++)
        {
            GameObject grass = Instantiate(GrassPrefs[0]);
            grass.transform.position = new Vector3(Random.Range(-Range, Range), 0, Random.Range(-Range, Range));
            grass.transform.rotation = Quaternion.Euler(0, Random.Range(-GrassRotation, GrassRotation), 0);
            grass.transform.parent = Environment.transform;
        }
        for (int i = 0; i < BigGrassAmount; i++)
        {
            GameObject grass = Instantiate(GrassPrefs[1]);
            grass.transform.position = new Vector3(Random.Range(-Range, Range), 0, Random.Range(-Range, Range));
            grass.transform.rotation = Quaternion.Euler(0, Random.Range(-GrassRotation, GrassRotation), 0);
            grass.transform.parent = Environment.transform;
        }
    }
}
