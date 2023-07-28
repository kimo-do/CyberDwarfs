using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Anvil : MonoBehaviour
{
    public static Anvil instance;

    public TMPro.TMP_Text anvilText;

    private void Awake()
    {
        instance = this;
    }
}
