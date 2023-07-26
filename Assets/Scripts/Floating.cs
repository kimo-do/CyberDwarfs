using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Floating : MonoBehaviour
{
    [SerializeField] private float speed = 5f;

    private Rigidbody2D rb;


    public Rigidbody2D Rb { get => rb; set => rb = value; }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
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
            Vector2 directionTowardsPlayer = DwarfController.instance.transform.position - transform.position;
            rb.AddForce(directionTowardsPlayer.normalized * speed, ForceMode2D.Force);
        }
    }
}
