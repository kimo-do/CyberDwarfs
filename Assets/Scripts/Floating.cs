using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Floating : MonoBehaviour
{
    [SerializeField] private float speed = 5f;

    private Rigidbody2D rb;
    private Enemy enemy;
    

    public Rigidbody2D Rb { get => rb; set => rb = value; }

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
                        float distanceToPlayer = Vector2.Distance(DwarfController.instance.transform.position, transform.position);

                        if (distanceToPlayer < 5f)
                        {
                            rb.drag = 10f;

                            if (!enemy.IsAttacking)
                            {
                                enemy.Attack();
                            }
                        }
                        else if (!enemy.IsAttacking) 
                        {
                            rb.drag = 1f;
                            Vector2 directionTowardsPlayer = (DwarfController.instance.transform.position + Vector3.up) - transform.position;
                            rb.AddForce(directionTowardsPlayer.normalized * speed, ForceMode2D.Force);
                        }
                    }
                }
            }
        }
    }
}
