using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Portal : MonoBehaviour
{
    public List<Portal> connectedPortals;
    public ParticleSystem visualEffect;

    public bool IsActive { get; set; } = true;
    public bool IsLocked { get; set; }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.TryGetComponent(out DwarfController dwarf))
        {
            if (IsActive)
            {
                List<Portal> availablePortals = connectedPortals.Where(p => !p.IsLocked).ToList();

                if (availablePortals.Count > 0)
                {
                    Portal rdnPortal = availablePortals[UnityEngine.Random.Range(0, availablePortals.Count)];
                    StartCoroutine(PortalOver(dwarf, rdnPortal));
                }
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.TryGetComponent(out DwarfController dwarf))
        {
            IsActive = true;
        }
    }

    public void DoEffect()
    {
        visualEffect.Play();
    }

    IEnumerator PortalOver(DwarfController dwarf, Portal toPortal)
    {
        Transform spawn = toPortal.transform.GetChild(0);
        toPortal.IsActive = false;
        IsActive = false;

        DoEffect();

        yield return new WaitForSeconds(0.2f);

        toPortal.DoEffect();

        yield return new WaitForSeconds(0.1f);

        dwarf.transform.position = spawn.position;

        yield return new WaitForEndOfFrame();

        IsActive = true;
    }

}
