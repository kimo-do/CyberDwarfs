using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TarodevController;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using static UnityEngine.Rendering.DebugUI;

public class DwarfGameManager : MonoBehaviour
{
    public static DwarfGameManager instance;

    public List<Upgrade> upgradePool;

    [SerializeField] private Transform spawnPoint;
    [SerializeField] private GameObject enemyPfb;
    [SerializeField] private GameObject slimeEnemyPfb;
    [SerializeField] private GameObject bulletPfb;
    [SerializeField] private GameObject allyBulletPfb;
    [SerializeField] private List<Transform> enemySpawns;
    [SerializeField] private List<Transform> enemySlimeSpawns;

    public Volume globalVolume;


    [Header("Settings")]
    [SerializeField] private int defaultLives;

    public int Lives { get; set; }

    public int Components { get; set; }
    public bool IsPlayerDeath { get => isPlayerDeath; set => isPlayerDeath = value; }
    public List<Upgrade> AppliedUpgrades { get => appliedUpgrades; set => appliedUpgrades = value; }

    private float lastEnemySpawn;
    private float lastSlimeSpawn;
    private float slimeSpawnTimer = 30f;
    private float orbSpawnTimer = 15f;
    private float lastGameDifficultyIncrease;
    private bool isPlayerDeath;
    private ChromaticAberration chrome;
    private ColorAdjustments colorAdjust;
    private Coroutine chromeRoutine;
    private Coroutine upgradesRoutine;
    private List<Upgrade> appliedUpgrades = new();
    private Dictionary<Transform, int> spawnedSlimes = new Dictionary<Transform, int>();

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
        lastGameDifficultyIncrease = Time.time;
    }    

    public void SpawnBullet(Vector2 from, Vector2 direction, int damage, bool ally = false)
    {
        GameObject newBullet = Instantiate(ally ? allyBulletPfb : bulletPfb, from, Quaternion.identity);
        newBullet.GetComponent<Bullet>().Damage = damage;
        newBullet.transform.right = direction;

        float forceMultiplier = ally ? DwarfController.instance.BulletSpeed : DwarfController.instance.OrbBulletSpeed;

        newBullet.GetComponent<Rigidbody2D>().AddForce(newBullet.transform.right * forceMultiplier, ForceMode2D.Impulse);

        if (ally)
        {
            newBullet.layer = LayerMask.NameToLayer("Bullet");
        }
        else
        {
            newBullet.layer = LayerMask.NameToLayer("EnemyBullet");
        }
    }

    private void Initialize()
    {
        if (upgradesRoutine != null)
        {
            StopCoroutine(upgradesRoutine);
        }

        upgradesRoutine = StartCoroutine(RunAllUpgrades());

        PlayerController.instance.enabled = true;
        DwarfController.instance.Rb.isKinematic = false;
        colorAdjust.saturation.value = 0f;
        isPlayerDeath = false;
        Lives = defaultLives;

        MenuController.instance.InitLives(Lives);
        MenuController.instance.SetCompononents(Components);

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
        DwarfController.instance.Rb.isKinematic = true;
        DwarfController.instance.Rb.velocity = Vector2.zero;
        DwarfController.instance.enabled = false;
        PlayerController.instance.Die();

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
        if (Time.time - lastEnemySpawn > orbSpawnTimer)
        {
            GameObject enemy = Instantiate(enemyPfb);
            Enemy enemyScript = enemy.GetComponent<Enemy>();
            enemyScript.ID = Enemy.NewId;
            Vector2 rdnspawnpos = enemySpawns[UnityEngine.Random.Range(0, enemySpawns.Count)].position;
            enemy.transform.position = rdnspawnpos;
            lastEnemySpawn = Time.time;
        }

        if (Time.time - lastSlimeSpawn > slimeSpawnTimer)
        {
            List<Transform> availableSlimeSpawns = enemySlimeSpawns.Where(s => !spawnedSlimes.ContainsKey(s)).ToList();

            if (availableSlimeSpawns.Count > 0)
            {
                GameObject enemy = Instantiate(slimeEnemyPfb);
                Transform rdnSpawn = availableSlimeSpawns[UnityEngine.Random.Range(0, availableSlimeSpawns.Count)];
                Vector2 rdnspawnpos = rdnSpawn.position;
                enemy.transform.position = rdnspawnpos;
                Enemy enemyScript = enemy.GetComponent<Enemy>();
                enemyScript.ID = Enemy.NewId;
                spawnedSlimes[rdnSpawn] = enemyScript.ID;
            }

            lastSlimeSpawn = Time.time;

        }

        if (Time.time - lastGameDifficultyIncrease > 60f)
        {
            slimeSpawnTimer *= 0.8f;
            orbSpawnTimer *= 0.8f;
            lastGameDifficultyIncrease = Time.time;
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

    public void OnEnemyDied(int enemyID)
    {
        foreach (var item in spawnedSlimes.Where(kvp => kvp.Value == enemyID).ToList())
        {
            spawnedSlimes.Remove(item.Key);
        }

        Components++;
        MenuController.instance.SetCompononents(Components);
    }


    IEnumerator ChromeHitEffect(Action DoneHit)
    {
        while (chrome.intensity.value < 1f)
        {
            chrome.intensity.value = chrome.intensity.value + Time.deltaTime * 15f;
            yield return null;
        }

        while (chrome.intensity.value > 0f)
        {
            chrome.intensity.value = chrome.intensity.value - Time.deltaTime * 15f;
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

        //Initialize();
        MenuController.instance.LoadFirstScene();
    }

    public void ApplyUpgrade(Upgrade upgrade)
    {
        if (!AppliedUpgrades.Contains(upgrade))
        {
            AppliedUpgrades.Add(upgrade);
            MenuController.instance.AddUpgrade();
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
            // Dash
            case 6:
                PlayerController.instance.PlayerStats.AllowDash = true;
                break;
            // Extra bullet damage
            case 7:
                DwarfController.instance.ShootDamage += (int)(DwarfController.instance.ShootDamage * 0.2f);
                break;
            // Extra bullet damage
            case 8:
                DwarfController.instance.ShootDamage += (int)(DwarfController.instance.ShootDamage * 0.2f);
                break;
            // Extra bullet damage
            case 9:
                DwarfController.instance.ShootDamage += (int)(DwarfController.instance.ShootDamage * 0.2f);
                break;
            // 20% more atk damage
            case 10:
                DwarfController.instance.Damage += (int)(DwarfController.instance.Damage * 0.2f);
                break;
            // 20% more atk damage
            case 11:
                DwarfController.instance.Damage += (int)(DwarfController.instance.Damage * 0.2f);
                break;
            // Increase bullet speed
            case 12:
                DwarfController.instance.BulletSpeed += (int)(DwarfController.instance.BulletSpeed * 0.2f);
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
