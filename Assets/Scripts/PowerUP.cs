using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerUP : MonoBehaviour
{
    public float defaultRechargeTime = 60f;
   
    public float RechargeTime { get; set; }

    private bool isActive = true;
    private float lastTakenTime;

    private void Awake()
    {
        RechargeTime = defaultRechargeTime;
    }

    public bool IsActive
    {
        get
        {
            return isActive;
        }

        set
        {
            isActive = value;

            if (isActive)
            {
                particlesActive.gameObject.SetActive(true);
            }
            else
            {
                particlesActive.gameObject.SetActive(false);
            }
        }
    }

    private void Update()
    {
        if (Time.time - lastTakenTime >= RechargeTime)
        {
            if (!isActive)
            {
                IsActive = true;
            }
        }
    }

    public ParticleSystem particlesActive;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isActive)
        {
            if (collision.gameObject.TryGetComponent(out DwarfController dwarf))
            {
                DwarfGameManager.instance.GainLive();
                IsActive = false;
                lastTakenTime = Time.time;
            }
        }
    }

}
