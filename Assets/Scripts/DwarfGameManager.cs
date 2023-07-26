using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DwarfGameManager : MonoBehaviour
{
    public static DwarfGameManager instance;

    [SerializeField] private Transform spawnPoint;
    [SerializeField] private RectTransform livesRect;
    [SerializeField] private GameObject heartTemplate;
    [SerializeField] private GameObject armourTemplate;
    [SerializeField] private GameObject enemyPfb;
    [SerializeField] private List<Transform> enemySpawns;

    [Header("Settings")]
    [SerializeField] private int defaultLives;

    public int Lives { get; set; }

    private List<GameObject> spawnedLivesUI = new();
    private List<GameObject> spawnedArmourUI = new();

    private float lastEnemySpawn;

    private void Awake()
    {
        instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        Initialize();
    }    

    private void Initialize()
    {
        Lives = defaultLives;

        for (int i = 0; i < spawnedLivesUI.Count; i++)
        {
            Destroy(spawnedLivesUI[i]);
        }

        for (int i = 0; i < spawnedArmourUI.Count; i++)
        {
            Destroy(spawnedArmourUI[i]);
        }

        for (int i = 0; i < Lives; i++)
        {
            GameObject spawnedLive = Instantiate(heartTemplate, heartTemplate.transform.parent);
            spawnedLive.SetActive(true);
            spawnedLivesUI.Add(spawnedLive);
        }

        DwarfController.instance.transform.position = spawnPoint.position;
    }

    public void LooseLive()
    {
        Lives--;

        if (spawnedLivesUI.Count > 0)
        {
            GameObject liveUI = spawnedLivesUI[^1];
            spawnedLivesUI.Remove(liveUI);
            Destroy(liveUI);
        }

        if (Lives <= 0)
        {
            PlayerDeath();
        }
    }

    public void PlayerDeath()
    {
        Initialize();
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.time - lastEnemySpawn > 10f)
        {
            GameObject enemy = Instantiate(enemyPfb);

            Vector2 rdnspawnpos = enemySpawns[Random.Range(0, enemySpawns.Count)].position;
            enemy.transform.position = rdnspawnpos;
            lastEnemySpawn = Time.time;
        }
    }
}
