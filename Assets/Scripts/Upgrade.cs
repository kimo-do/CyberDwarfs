using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Upgrade
{
    public int ID;
    public UpgradeType Type;
    public string Title;
    public string Description;

    public enum UpgradeType
    {
        Misc,
        Melee,
        Ranged,
    }
}
