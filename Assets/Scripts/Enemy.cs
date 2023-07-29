using System.Collections;
using System.Collections.Generic;
using TarodevController;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] private SpriteRenderer hitOverlay;
    [SerializeField] private Transform shootPoint;


    [Header("Settings")]
    [SerializeField] private int defaultHealth = 100;

    public EnemyType enemyType;
    public float attackInterval = 2f;

    public int CurrentHealth { get; set; }
    public int MaxHealth { get; set; }

    public float LastHitTime { get; set; }
    public float LastGotAttackedTime { get; set; }

    public float LastAttackTime { get; set; }

    public bool IsAttacking { get; set; }

    private SpriteRenderer sr;
    private Coroutine flashWhiteRoutine;
    private Coroutine attackingRoutine;
    private Rigidbody2D rb;
    private Floating floating;
    private Animator anim;

    public bool hasDied = false;


    public enum EnemyType
    {
        None,
        Orb,
        Goblin,
        Slime
    }

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        floating = GetComponent<Floating>();
        anim = GetComponent<Animator>();

        Initialize();
    }

    private void Update()
    {
        //if (rb.velocity.x > 0)
        //{
        //    sr.flipX = true;
        //}
        //else if (rb.velocity.x < 0)
        //{
        //    sr.flipX = false;
        //}

        Vector2 relPlayerPos = DwarfController.instance.transform.InverseTransformPoint(transform.position);

        if (relPlayerPos.x > 0)
        {
            sr.flipX = false;
        }
        else if (relPlayerPos.x < 0)
        {
            sr.flipX = true;
        }
    }

    void Initialize()
    {
        CurrentHealth = defaultHealth;
        MaxHealth = defaultHealth;
    }

    public void Bounce(Vector2 direction)
    {
        rb.AddForce(direction.normalized * 10f, ForceMode2D.Impulse);
    }

    public void GetHit(int damage, bool whileGrounded = true)
    {
        CurrentHealth -= damage;

        if (flashWhiteRoutine != null)
        {
            StopCoroutine(flashWhiteRoutine);
        }

        flashWhiteRoutine = StartCoroutine(FlashWhite());

        if (CurrentHealth <= 0)
        {
            CurrentHealth = 0;

            if (!hasDied)
            {
                Died();
            }
        }

        if (!whileGrounded)
        {
            rb.velocity = Vector2.zero;
        }
        LastGotAttackedTime = Time.time;
    }

    IEnumerator FlashWhite()
    {
        float startTime = Time.time;

        while (hitOverlay.color.a < 1f)
        {
            hitOverlay.color = new Color(1f, 1f, 1f, hitOverlay.color.a + Time.deltaTime * 20f);
            yield return null;
        }

        while (hitOverlay.color.a > 0f)
        {
            hitOverlay.color = new Color(1f, 1f, 1f, hitOverlay.color.a - Time.deltaTime * 20f);
            yield return null;
        }
    }

    private void Died()
    {
        DwarfGameManager.instance.OnEnemyDied();

        if (anim != null)
        {
            anim.SetTrigger("Death");
        }

        hasDied = true;

        Collider2D col = GetComponent<Collider2D>();

        if (col != null)
        {
            col.enabled = false;
        }

        rb.isKinematic = true;

        if (enemyType != EnemyType.Orb)
        {
            rb.velocity = Vector2.zero;
        }

        rb.drag = 0f;
    }

    public void DeathFinished()
    {
        Destroy(gameObject);
    }

    public void Attack()
    {
        if (Time.time - LastAttackTime > attackInterval)
        {
            if (!DwarfGameManager.instance.IsPlayerDeath)
            {
                IsAttacking = true;
                if (attackingRoutine != null)
                {
                    StopCoroutine(attackingRoutine);
                }

                switch (enemyType)
                {
                    case EnemyType.Orb:
                        attackingRoutine = StartCoroutine(OrbAttack());
                        break;
                }
            }
        }
    }

    IEnumerator OrbAttack()
    {
        if (anim != null)
        {
            anim.SetTrigger("Attacking");
        }

        yield return new WaitForSeconds(0.5f);

        Vector2 directionToPlayer = DwarfController.instance.hitCircle.transform.position - transform.position;
        DwarfGameManager.instance.SpawnBullet(shootPoint.transform.position, directionToPlayer, 1);

        LastAttackTime = Time.time; 
        IsAttacking = false;
        yield return null;
    }
}
