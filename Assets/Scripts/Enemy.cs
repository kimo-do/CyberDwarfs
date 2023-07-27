using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] private SpriteRenderer hitOverlay;

    [Header("Settings")]
    [SerializeField] private int defaultHealth = 100;

    public int CurrentHealth { get; set; }
    public int MaxHealth { get; set; }

    public float LastHitTime { get; set; }
    public float LastGotAttackedTime { get; set; }

    private SpriteRenderer sr;
    private Coroutine flashWhiteRoutine;
    private Rigidbody2D rb;
    private Floating floating;

    public bool hasDied = false;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        floating = GetComponent<Floating>();

        Initialize();
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
        hasDied = true;

        Collider2D col = GetComponent<Collider2D>();

        if (col != null)
        {
            col.enabled = false;
        }

        rb.gravityScale = 1.5f;
        rb.drag = 0f;
        sr.color = Color.gray;

        Destroy(gameObject, 6f);
    }
}
