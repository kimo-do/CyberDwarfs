using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UpgradeButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Button Button
    {
        get
        {
            return GetComponent<Button>();
        }
    }

    public Upgrade StoredUpgrade { get; set; }

    public void OnPointerEnter(PointerEventData eventData)
    {
        MenuController.instance.HoverUpgradeButton(this);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        MenuController.instance.StopHoverButton();
    }

    public void ClickedBtn()
    {
        MenuController.instance.ClickUpgradeButton(this);
    }

    public void SetUpgrade(Upgrade upgrade)
    {
        StoredUpgrade = upgrade;
        GetComponentInChildren<TextMeshProUGUI>().text = upgrade.Title;
        GetComponent<Image>().sprite = MenuController.instance.GetFromType(upgrade.Type);
    }
}
