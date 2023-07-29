using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public int Damage { get; set; }

    public bool HasDamaged { get; set; } = false;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (HasDamaged) return;

        if (collision.gameObject.TryGetComponent(out Enemy enemy))
        {
            enemy.GetHit(Damage);
        }
        if (collision.gameObject.TryGetComponent(out DwarfController dwarf))
        {
            DwarfGameManager.instance.LooseLive();
        }

        HasDamaged = true;

        transform.GetComponent<Rigidbody2D>().velocity = Vector3.zero;
        transform.GetComponent<Rigidbody2D>().isKinematic = true;
        ParticleSystem[] ps = transform.GetComponentsInChildren<ParticleSystem>();

        for (int i = 0; i < ps.Length; i++)
        {
            ps[i].Stop();
        }

        Destroy(gameObject, 2f);
    }

}
