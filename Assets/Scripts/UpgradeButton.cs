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
        //Animation anim = transform.parent.GetComponent<Animation>();
        //anim["ButtonHover"].time = 0;
        //anim["ButtonHover"].speed = 1f;
        //anim.Play("ButtonHover");
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        MenuController.instance.StopHoverButton();
        //Animation anim = transform.parent.GetComponent<Animation>();
        //anim["ButtonHover"].time = anim["ButtonHover"].length;
        //anim["ButtonHover"].speed = -1f;
        //anim.Play("ButtonHover");
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
