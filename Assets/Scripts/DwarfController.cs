using System.Collections;
using System.Collections.Generic;
using TarodevController;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class DwarfController : MonoBehaviour
{
    public static DwarfController instance;
    public HitArea hitArea;
    public Transform hitCircle;
    public Transform gun;
    public Transform gunNossle;
    public Animator swooshAnim;
    public ParticleSystem shieldEffect;

    [Header("Settings")]
    public int defaultDamage = 60;
    public int defaultBulletDamage = 40;
    public float defaultAllyBulletSpeed = 10f;
    public float defaultenemyBulletSpeed = 5f;


    public float defaultAttackTime = 0.6f;
    public float defaultShootTime = 0.6f;


    public int Damage { get; set; }
    public int ShootDamage { get; set; }
    public float BulletSpeed { get; set; }
    public float OrbBulletSpeed { get; set; }
    public float AttackTime { get; set; }
    public float ShootTime { get; set; }

    public bool Invulnerable {  get; set; }
    public Rigidbody2D Rb { get => rb; set => rb = value; }

    private float lastAttackTime;
    private float lastShootTime;


    private Rigidbody2D rb;

    private void Awake()
    {
        instance = this;
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        Initialize();
    }

    private void Initialize()
    {
        Damage = defaultDamage;
        ShootDamage = defaultBulletDamage;
        AttackTime = defaultAttackTime;
        ShootTime = defaultShootTime;
        BulletSpeed = defaultAllyBulletSpeed;
        OrbBulletSpeed = defaultenemyBulletSpeed;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.TryGetComponent(out Enemy enemy))
        {
            if (enemy.enemyType == Enemy.EnemyType.Slime)
            {
                if (Time.time - enemy.LastHitTime > 0.5f)
                {
                    if (Time.time - enemy.LastGotAttackedTime > 0.6f)
                    {
                        enemy.LastHitTime = Time.time;

                        Vector2 awayFromPlayerDir = transform.position - enemy.transform.position;
                        PlayerController.instance.ApplyVelocity(awayFromPlayerDir * 50f, PlayerForce.Burst);
                        //enemy.Bounce(awayFromPlayerDir);


                            DwarfGameManager.instance.LooseLive();
                        
                    }
                }
            }
        }
    }

    private void Update()
    {
        // Basic attack
        if (!IsPointerOverUIElement())
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (Time.time - lastAttackTime >= AttackTime)
                {
                    DoAttack();
                    lastAttackTime = Time.time;
                }
            }

            // Shoot attack
            if (Input.GetMouseButtonDown(1))
            {
                if (Time.time - lastShootTime >= ShootTime)
                {
                    DoShoot();
                    lastShootTime = Time.time;
                }
            }
        }

        Vector2 mousePosWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 lookAtMouse = mousePosWorld - (Vector2)hitCircle.position;
        hitCircle.right = lookAtMouse;
        gun.right = lookAtMouse;
    }

    //Returns 'true' if we touched or hovering on Unity UI element.
    public static bool IsPointerOverUIElement()
    {
        return IsPointerOverUIElement(GetEventSystemRaycastResults());
    }


    //Returns 'true' if we touched or hovering on Unity UI element.
    public static bool IsPointerOverUIElement(List<RaycastResult> eventSystemRaysastResults)
    {
        for (int index = 0; index < eventSystemRaysastResults.Count; index++)
        {
            RaycastResult curRaysastResult = eventSystemRaysastResults[index];
            if (curRaysastResult.gameObject.layer == LayerMask.NameToLayer("UI"))
                return true;
        }
        return false;
    }


    //Gets all event system raycast results of current mouse or touch position.
    public static List<RaycastResult> GetEventSystemRaycastResults()
    {
        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = Input.mousePosition;
        List<RaycastResult> raysastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, raysastResults);
        return raysastResults;
    }

    private Coroutine hideGunRoutine;

    private void DoShoot()
    {
        DwarfGameManager.instance.SpawnBullet(gunNossle.position, gun.right, ShootDamage, true);
        gun.gameObject.SetActive(true);

        if (hideGunRoutine != null)
        {
            StopCoroutine(hideGunRoutine);
        }

        hideGunRoutine = StartCoroutine(HideGun());
    }

    IEnumerator HideGun()
    {
        yield return new WaitForSeconds(0.3f);
        gun.gameObject.SetActive(false);
    }

    private void DoAttack()
    {
        List<GameObject> hits = hitArea.GetEnemyCollisions();

        GameObject swoosh = Instantiate(swooshAnim.gameObject, swooshAnim.transform.position, swooshAnim.transform.rotation);
        swoosh.GetComponent<Animator>().SetTrigger("Slash");
        Destroy(swoosh, 0.25f);

        if (hits.Count > 0)
        {
            foreach (var enemyGO in hits)
            {
                if (enemyGO.TryGetComponent(out Enemy enemy))
                {
                    Vector2 awayFromPlayerDir = enemy.transform.position - hitCircle.position;

                    enemy.GetHit(Damage, PlayerController.instance.Grounded);
                    PlayerController.instance.startHoverTime = Time.time;

                    if (PlayerController.instance.Grounded)
                    {
                        enemy.Bounce(awayFromPlayerDir);
                    }
                }
            }
        }
    }

}
