using System;
using System.Collections;
using System.Collections.Generic;
using TarodevController;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class DwarfGameManager : MonoBehaviour
{
    public static DwarfGameManager instance;

    [SerializeField] private Transform spawnPoint;
    [SerializeField] private GameObject enemyPfb;
    [SerializeField] private List<Transform> enemySpawns;
    public Volume globalVolume;


    [Header("Settings")]
    [SerializeField] private int defaultLives;

    public int Lives { get; set; }
    public bool IsPlayerDeath { get => isPlayerDeath; set => isPlayerDeath = value; }

    private float lastEnemySpawn;
    private bool isPlayerDeath;
    private ChromaticAberration chrome;
    private ColorAdjustments colorAdjust;
    private Coroutine chromeRoutine;


    private void Awake()
    {
        instance = this;

        globalVolume.profile.TryGet(out chrome);
        globalVolume.profile.TryGet(out colorAdjust);
    }

    // Start is called before the first frame update
    void Start()
    {
        Initialize();
    }    

    private void Initialize()
    {
        PlayerController.instance.enabled = true;
        colorAdjust.saturation.value = 0f;
        isPlayerDeath = false;
        Lives = defaultLives;

        MenuController.instance.InitLives(Lives);

        DwarfController.instance.transform.position = spawnPoint.position;
    }

    public void LooseLive()
    {
        if (isPlayerDeath) return;

        Lives--;

        MenuController.instance.LooseLive();

        if (Lives <= 0)
        {
            PlayerDeath();
        }
        else
        {
            if (chromeRoutine != null)
            {
                StopCoroutine(chromeRoutine);
            }

            chromeRoutine = StartCoroutine(ChromeHitEffect(null));
        }
    }

    public void PlayerDeath()
    {
        isPlayerDeath = true;
        PlayerController.instance.enabled = false;

        if (chromeRoutine != null)
        {
            StopCoroutine(chromeRoutine);
        }

        chromeRoutine = StartCoroutine(ChromeHitEffect(() =>
        {
            StartCoroutine(DeathEffect());
        }));
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.time - lastEnemySpawn > 10f)
        {
            GameObject enemy = Instantiate(enemyPfb);

            Vector2 rdnspawnpos = enemySpawns[UnityEngine.Random.Range(0, enemySpawns.Count)].position;
            enemy.transform.position = rdnspawnpos;
            lastEnemySpawn = Time.time;
        }
    }


    IEnumerator ChromeHitEffect(Action DoneHit)
    {
        while (chrome.intensity.value < 1f)
        {
            chrome.intensity.value = chrome.intensity.value + Time.deltaTime * 20f;
            yield return null;
        }

        while (chrome.intensity.value > 0f)
        {
            chrome.intensity.value = chrome.intensity.value - Time.deltaTime * 20f;
            yield return null;
        }

        DoneHit?.Invoke();
    }

    IEnumerator DeathEffect()
    {
        while (colorAdjust.saturation.value > -100f)
        {
            colorAdjust.saturation.value = colorAdjust.saturation.value - Time.deltaTime * 60f;
            yield return null;
        }

        yield return new WaitForSeconds(2f);

        Initialize();
    }

}
