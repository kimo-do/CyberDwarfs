using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DwarfGameManager : MonoBehaviour
{
    [SerializeField] private Transform spawnPoint;

    [Header("Settings")]
    [SerializeField] private int defaultLives;

    public int Lives { get; set; }

    // Start is called before the first frame update
    void Start()
    {
        Lives = defaultLives;
    }

    public void LooseLive()
    {
        Lives--;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
