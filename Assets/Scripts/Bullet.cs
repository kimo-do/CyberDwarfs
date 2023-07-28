using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public int Damage { get; set; }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.TryGetComponent(out Enemy enemy))
        {
            enemy.GetHit(Damage);
        }
        if (collision.gameObject.TryGetComponent(out DwarfController dwarf))
        {
            DwarfGameManager.instance.LooseLive();
        }

        Destroy(gameObject);
    }

}
