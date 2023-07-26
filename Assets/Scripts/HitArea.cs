using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HitArea : MonoBehaviour
{
    [SerializeField] DwarfController dwarfController;
    [SerializeField] private ContactFilter2D filter;

    private PolygonCollider2D col;

    private void Awake()
    {
        col = GetComponent<PolygonCollider2D>();
    }

    public List<GameObject> GetEnemyCollisions()
    {
        Collider2D[] hits =new Collider2D[10];

        if (col.OverlapCollider(filter, hits) > 0)
        {
            List<GameObject> enemiesHit = new();

            for (int i = 0; i < hits.Length; i++)
            {
                if (hits[i] != null)
                {
                    enemiesHit.Add(hits[i].gameObject);
                }
            }

            return enemiesHit;
        }

        return new();
    }

    //private void OnTriggerEnter2D(Collider2D collision)
    //{
    //    if (collision.gameObject.tag == "Enemy")
    //    {
    //        dwarfController.HitEnemy(collision.gameObject);
    //    }
    //}
}
