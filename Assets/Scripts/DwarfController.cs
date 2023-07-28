using System.Collections;
using System.Collections.Generic;
using TarodevController;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class DwarfController : MonoBehaviour
{
    public static DwarfController instance;
    public HitArea hitArea;
    public Transform hitCircle;
    public Transform gun;
    public Transform gunNossle;
    public Animation anim;

    [Header("Settings")]
    public int defaultDamage = 40;
    public int defaultBulletDamage = 30;

    public float defaultAttackTime = 0.6f;

    public int Damage { get; set; }
    public int ShootDamage { get; set; }
    public float AttackTime { get; set; }
    public Rigidbody2D Rb { get => rb; set => rb = value; }

    private float lastAttackTime;

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

                        Vector2 awayFromPlayerDir = enemy.transform.position - transform.position;
                        PlayerController.instance.ApplyVelocity(awayFromPlayerDir, PlayerForce.Burst);
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
            if (Time.time - lastAttackTime >= AttackTime)
            {
                DoShoot();
                lastAttackTime = Time.time;
            }
        }

        Vector2 mousePosWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 lookAtMouse = mousePosWorld - (Vector2)hitCircle.position;
        hitCircle.right = lookAtMouse;
        gun.right = lookAtMouse;
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

        anim["Hit"].speed = 3f;
        anim.Play();
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
