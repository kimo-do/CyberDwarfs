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
    public Animation anim;

    [Header("Settings")]
    public int defaultDamage = 40;
    public float defaultAttackTime = 0.6f;

    public int Damage { get; set; }
    public float AttackTime { get; set; }

    private float lastAttackTime;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        Initialize();
    }

    private void Initialize()
    {
        Damage = defaultDamage;
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

                        //Vector2 awayFromPlayerDir = enemy.transform.position - transform.position;
                        //enemy.Bounce(awayFromPlayerDir);

                        DwarfGameManager.instance.LooseLive();
                    }
                }
            }
        }
        if (collision.gameObject.TryGetComponent(out Bullet bullet))
        {
            Destroy(bullet.gameObject);
            DwarfGameManager.instance.LooseLive();
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

        Vector2 mousePosWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 lookAtMouse = mousePosWorld - (Vector2)hitCircle.position;
        hitCircle.right = lookAtMouse;
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
