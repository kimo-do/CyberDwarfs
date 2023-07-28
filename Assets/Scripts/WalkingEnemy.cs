using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WalkingEnemy : MonoBehaviour
{
    [SerializeField] private float speed = 5f;

    public Transform checkLeft;
    public Transform checkRight;

    private Rigidbody2D rb;
    private Enemy enemy;
    public Rigidbody2D Rb { get => rb; set => rb = value; }

    private Vector2 currentMoveDir = Vector2.right;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        enemy = GetComponent<Enemy>();
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (DwarfController.instance != null)
        {
            if (enemy != null)
            {
                if (!enemy.hasDied)
                {
                    if (Time.time - enemy.LastGotAttackedTime > 1f)
                    {
                        if (!enemy.IsAttacking)
                        {
                            RaycastHit2D currentHit = new RaycastHit2D();

                            if (currentMoveDir.x > 0)
                            {
                                currentHit = Physics2D.Raycast(checkRight.position, Vector2.down, 5f);
                            }
                            else if (currentMoveDir.x < 0)
                            {
                                currentHit = Physics2D.Raycast(checkLeft.position, Vector2.down, 5f);
                            }

                            if (currentHit.collider == null)
                            {
                                rb.velocity = Vector2.zero;
                                currentMoveDir *= -1;
                            }
                            else if (currentHit.distance > 0.5f)
                            {
                                rb.velocity = Vector2.zero;
                                currentMoveDir *= -1;
                            }

                            rb.velocity = new Vector2(currentMoveDir.x, rb.velocity.y);
                        }
                    }
                }
            }
        }
    }
}
