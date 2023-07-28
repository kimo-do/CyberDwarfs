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

    public List<Upgrade> upgradePool;

    [SerializeField] private Transform spawnPoint;
    [SerializeField] private GameObject enemyPfb;
    [SerializeField] private GameObject bulletPfb;
    [SerializeField] private List<Transform> enemySpawns;
    public Volume globalVolume;


    [Header("Settings")]
    [SerializeField] private int defaultLives;

    public int Lives { get; set; }
    public bool IsPlayerDeath { get => isPlayerDeath; set => isPlayerDeath = value; }
    public List<Upgrade> AppliedUpgrades { get => appliedUpgrades; set => appliedUpgrades = value; }

    private float lastEnemySpawn;
    private bool isPlayerDeath;
    private ChromaticAberration chrome;
    private ColorAdjustments colorAdjust;
    private Coroutine chromeRoutine;
    private Coroutine upgradesRoutine;
    private List<Upgrade> appliedUpgrades = new();

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

    public void SpawnBullet(Vector2 from, Vector2 direction)
    {
        GameObject newBullet = Instantiate(bulletPfb, from, Quaternion.identity);
        newBullet.transform.right = direction;
        newBullet.GetComponent<Rigidbody2D>().AddForce(newBullet.transform.right * 5f, ForceMode2D.Impulse);
    }

    private void Initialize()
    {
        if (upgradesRoutine != null)
        {
            StopCoroutine(upgradesRoutine);
        }

        upgradesRoutine = StartCoroutine(RunAllUpgrades());

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

    public void GainLive()
    {
        if (isPlayerDeath) return;

        Lives++;

        MenuController.instance.GainLive();
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

        if (Vector2.Distance(DwarfController.instance.transform.position, Anvil.instance.transform.position) < 2.5f)
        {
            Anvil.instance.anvilText.gameObject.SetActive(true);

            if (!isPlayerDeath)
            {
                if (Input.GetKeyDown(KeyCode.U))
                {
                    MenuController.instance.ClickedAnvil();
                }
            }
        }
        else
        {
            Anvil.instance.anvilText.gameObject.SetActive(false);
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

    public void ApplyUpgrade(Upgrade upgrade)
    {
        if (!AppliedUpgrades.Contains(upgrade))
        {
            AppliedUpgrades.Add(upgrade);
        }

        switch (upgrade.ID)
        {
                // Extra jump
            case 1:
                PlayerController.instance.PlayerStats.MaxAirJumps++;
                break;
                // 20% more atk damage
            case 2:
                DwarfController.instance.Damage += (int)(DwarfController.instance.Damage * 0.2f);
                break;
                // Increase jump height
            case 3:
                PlayerController.instance.PlayerStats.JumpPower += (int)(PlayerController.instance.PlayerStats.JumpPower * 0.2f);
                break;                
                // Increase running speed
            case 4:
                PlayerController.instance.PlayerStats.MaxSpeed += (int)(PlayerController.instance.PlayerStats.MaxSpeed * 0.2f);
                PlayerController.instance.PlayerStats.Acceleration += (int)(PlayerController.instance.PlayerStats.Acceleration * 0.2f);
                break;
            // Reduce falling speed
            case 5:
                PlayerController.instance.PlayerStats.MaxFallSpeed -= (int)(PlayerController.instance.PlayerStats.MaxFallSpeed * 0.2f);
                PlayerController.instance.PlayerStats.FallAcceleration -= (int)(PlayerController.instance.PlayerStats.FallAcceleration * 0.2f);
                break;
        }
    }

    IEnumerator RunAllUpgrades()
    {
        float restoreHealth1Time = Time.time;

        while (true)
        {
            foreach (var upgrade in AppliedUpgrades)
            {
                switch (upgrade.ID)
                {
                    // Restore health every 90 sec
                    case 0:
                        if (Time.time - restoreHealth1Time >= 90)
                        {
                            GainLive();
                            restoreHealth1Time = Time.time;
                        }
                        break;
                }
            }

            yield return new WaitForEndOfFrame();
        }
    }

}
