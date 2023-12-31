using Cysharp.Threading.Tasks;
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
    public GameObject anvilEffect;
    public PowerUP powerup1;
    public PowerUP powerup2;
    [SerializeField] private List<Transform> enemySpawns;
    [SerializeField] private List<Transform> enemySlimeSpawns;

    public List<VisualNFT> ingameNftsScreens;
    private Dictionary<VisualNFT, bool> usedScreens = new();

    public GameObject crazyModeBG;

    public Volume globalVolume;
    public Volume crazyModeVolume;


    [Header("Settings")]
    [SerializeField] private int defaultLives;

    public int TotalKills { get; set; }
    public int Lives { get; set; }

    public int Components = 0;
    public bool IsPlayerDeath { get => isPlayerDeath; set => isPlayerDeath = value; }

    public List<Upgrade> AppliedUpgrades { get => appliedUpgrades; set => appliedUpgrades = value; }

    private float lastEnemySpawn;
    private float lastSlimeSpawn;
    private float slimeSpawnTimer = 20f;
    private float orbSpawnTimer = 12f;
    private int maxLives = 10;
    private float lastGameDifficultyIncrease;
    private bool isPlayerDeath;
    private ChromaticAberration chrome;
    private ColorAdjustments colorAdjust;
    private Coroutine chromeRoutine;
    private Coroutine upgradesRoutine;
    private List<Upgrade> appliedUpgrades = new();
    private Dictionary<Transform, int> spawnedSlimes = new Dictionary<Transform, int>();

    public VisualNFT GetUnusedScreen()
    {
        if (usedScreens.ToList().Any(s => s.Value == false))
        {
            VisualNFT vnft = usedScreens.FirstOrDefault(s => s.Value == false).Key;
            usedScreens[vnft] = true;
            return vnft;
        }

        return null;
    }

    private void Awake()
    {
        instance = this;

        globalVolume.profile.TryGet(out chrome);
        globalVolume.profile.TryGet(out colorAdjust);

        foreach (var s in ingameNftsScreens)
        {
            usedScreens[s] = false;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        Initialize();
        lastGameDifficultyIncrease = Time.time;
        lastEnemySpawn = Time.time - 200f;
        StartCoroutine(ShowNFTS());
    }
    
    IEnumerator ShowNFTS()
    {
        yield return new WaitForSeconds(2f);

        foreach (var fetchednft in NFTFetcher.instance.FetchedNFTs)
        {
            VisualNFT visualNFT = GetUnusedScreen();

            if (visualNFT != null)
            {
                visualNFT.SetNFT(fetchednft.account, fetchednft.NFT);
            }
        }
    }

    private bool isInCrazyMode = false;

    public void EnableCrazyMode()
    {
        if (!isInCrazyMode)
        {
            isInCrazyMode = true;
            StartCoroutine(CrazyMode());
        }
    }

    IEnumerator CrazyMode()
    {
        while(crazyModeVolume.weight < 1f)
        {
            crazyModeVolume.weight += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        crazyModeBG.gameObject.SetActive(true);
        DwarfController.instance.shieldEffect.gameObject.SetActive(true);
        MenuController.instance.overloadText.gameObject.SetActive(true);

        float currentAtkTime = DwarfController.instance.AttackTime;
        float currentShootTime = DwarfController.instance.ShootTime;
        int currentAtkDmg = DwarfController.instance.Damage;
        int currentShootDmg = DwarfController.instance.ShootDamage;
        int currentAirJumps = PlayerController.instance.PlayerStats.MaxAirJumps;

        DwarfController.instance.Invulnerable = true;
        DwarfController.instance.AttackTime = 0.1f;
        DwarfController.instance.ShootTime = 0.1f;
        DwarfController.instance.Damage = 200;
        DwarfController.instance.ShootDamage = 200;
        PlayerController.instance.PlayerStats.MaxAirJumps = 10;
        PlayerController.instance.ResetAirJumps();


        yield return new WaitForSeconds(20f);


        DwarfController.instance.Invulnerable = false;
        DwarfController.instance.AttackTime = currentAtkTime;
        DwarfController.instance.ShootTime = currentShootTime;
        DwarfController.instance.Damage = currentAtkDmg;
        DwarfController.instance.ShootDamage = currentShootDmg;
        PlayerController.instance.PlayerStats.MaxAirJumps = currentAirJumps;
        PlayerController.instance.ResetAirJumps();

        DwarfController.instance.shieldEffect.gameObject.SetActive(false);
        MenuController.instance.overloadText.gameObject.SetActive(false);


        crazyModeBG.gameObject.SetActive(false);

        while (crazyModeVolume.weight > 0f)
        {
            crazyModeVolume.weight -= Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        isInCrazyMode = false;
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
        MenuController.instance.ClearUpgrades();

        DwarfController.instance.transform.position = spawnPoint.position;
    }

    public void LooseLive()
    {
        if (isPlayerDeath) return;
        if (DwarfController.instance.Invulnerable) return;

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

        if (Lives < maxLives)
        {
            Lives++;
            MenuController.instance.GainLive();
        }
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

            if (Enemy.OrbAttackInterval == 0)
            {
                Enemy.OrbAttackInterval = enemyScript.attackInterval;
            }

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

                Transform biggestDistance = availableSlimeSpawns[0];
                float distance = 0;

                foreach (var sp in availableSlimeSpawns)
                {
                    float newDistance = Vector2.Distance(sp.position, DwarfController.instance.transform.position);

                    if (newDistance > distance)
                    {
                        distance = newDistance;
                        biggestDistance = sp;
                    }
                }

                Transform rdnSpawn = biggestDistance; //availableSlimeSpawns[UnityEngine.Random.Range(0, availableSlimeSpawns.Count)];
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
            Enemy.OrbAttackInterval *= 0.9f;
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
        TotalKills++;

        if (Components >= 3)
        {
            anvilEffect.gameObject.SetActive(true);
        }

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

        yield return new WaitForSeconds(1f);

        CanvasGroup group = MenuController.instance.deathscreen.GetComponent<CanvasGroup>();

        MenuController.instance.killsText.text = "Total kills: " + TotalKills.ToString();

        while (group.alpha < 1f)
        {
            group.alpha += Time.deltaTime * 0.5f;
            yield return new WaitForEndOfFrame();
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
                PlayerController.instance.ResetAirJumps();
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
            // Increase bullet speed
            case 13:
                DwarfController.instance.BulletSpeed += (int)(DwarfController.instance.BulletSpeed * 0.2f);
                break;
            // Shoot more frequently
            case 14:
                DwarfController.instance.ShootTime -= (int)(DwarfController.instance.ShootTime * 0.2f);
                break;
            // Shoot more frequently
            case 15:
                DwarfController.instance.ShootTime -= (int)(DwarfController.instance.ShootTime * 0.2f);
                break;
            // Attack more frequently
            case 16:
                DwarfController.instance.AttackTime -= (int)(DwarfController.instance.AttackTime * 0.2f);
                break;
            // Attack more frequently
            case 17:
                DwarfController.instance.AttackTime -= (int)(DwarfController.instance.AttackTime * 0.2f);
                break;
            // Gain extra lives
            case 18:
                GainLive();
                break;
            // Gain extra lives
            case 19:
                GainLive();
                GainLive();
                break;
            // Gain extra lives
            case 20:
                GainLive();
                GainLive();
                GainLive();
                break;
        }

        if (AppliedUpgrades.Count >= upgradePool.Count)
        {
            EnableCrazyMode();
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
