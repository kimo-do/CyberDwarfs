using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OutOfBounds : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.transform.root == DwarfController.instance.transform)
        {
            DwarfGameManager.instance.PlayerDeath();
        }
        else if (collision.gameObject.TryGetComponent(out Enemy enemy))
        {
            Destroy(enemy.gameObject);
        }
    }
}
