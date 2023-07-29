using System;
using System.Collections;
using System.Collections.Generic;
using TarodevController;
using TMPro;
using UnityEngine;
using UnityEngine.Diagnostics;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static Cinemachine.DocumentationSortingAttribute;

public class MenuController : MonoBehaviour
{
    public static MenuController instance;

    public RectTransform introScreen;
    public RectTransform mainGameScreen;
    public RectTransform upgradeScreen;
    public CanvasGroup fadeToBlack;
    public Animation backpackTip;
    public float fadeSpeed = 1f;

    // Main UI
    [SerializeField] private RectTransform livesRect;
    [SerializeField] private GameObject heartTemplate;
    [SerializeField] private GameObject armourTemplate;

    // Upgrades
    public UpgradeButton upgrade1btn;
    public UpgradeButton upgrade2btn;
    public UpgradeButton upgrade3btn;
    public TextMeshProUGUI explainText;
    public Sprite meleeIcon;
    public Sprite rangedIcon;
    public Sprite miscIcon;

    private List<GameObject> spawnedLivesUI = new();
    private List<GameObject> spawnedArmourUI = new();

    private bool startedGame = false;

    public bool HasOpenAnvil { get; set; }

    public Sprite GetFromType(Upgrade.UpgradeType type)
    {
        switch (type)
        {
            case Upgrade.UpgradeType.Ranged: return rangedIcon;
            case Upgrade.UpgradeType.Melee: return meleeIcon;
            case Upgrade.UpgradeType.Misc: return miscIcon;
        }

        return null;
    }

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        DontDestroyOnLoad(gameObject);
        introScreen.gameObject.SetActive(true);

        StartCoroutine(ShowTipAfterWhile());
    }

    IEnumerator ShowTipAfterWhile()
    {
        yield return new WaitForSeconds(6f);

        if (Screen.width < Screen.height)
        {
            backpackTip["Tip"].time = 0;
            backpackTip["Tip"].speed = 1f;
            backpackTip.Play("Tip");

            yield return new WaitForSeconds(7f);

            backpackTip["Tip"].time = backpackTip["Tip"].length;
            backpackTip["Tip"].speed = -1f;
            backpackTip.Play("Tip");
        }
    }

    public void StartGame()
    {
        if (startedGame) return;

        startedGame = true;

        StartCoroutine(FadeToBlack(() =>
        {
            LoadFirstScene();
        }));
    }

    public void LoadFirstScene()
    {
        introScreen.gameObject.SetActive(false);
        StartCoroutine(LoadFirstSceneAsync());
    }

    IEnumerator LoadFirstSceneAsync()
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("Main v2");

        // Wait until the asynchronous scene fully loads
        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        mainGameScreen.gameObject.SetActive(true);

        StartCoroutine(FadeFromBlack(null));
    }

    IEnumerator FadeToBlack(Action AfterFade)
    {
        while (fadeToBlack.alpha < 1)
        {
            fadeToBlack.alpha += Time.unscaledDeltaTime * fadeSpeed;
            yield return null;
        }

        AfterFade?.Invoke();
    }

    IEnumerator FadeFromBlack(Action AfterFade)
    {
        while (fadeToBlack.alpha > 0)
        {
            fadeToBlack.alpha -= Time.unscaledDeltaTime * fadeSpeed;
            yield return null;
        }

        AfterFade?.Invoke();
    }

    public void InitLives(int Lives)
    {
        for (int i = 0; i < spawnedLivesUI.Count; i++)
        {
            Destroy(spawnedLivesUI[i]);
        }

        for (int i = 0; i < spawnedArmourUI.Count; i++)
        {
            Destroy(spawnedArmourUI[i]);
        }

        for (int i = 0; i < Lives; i++)
        {
            GameObject spawnedLive = Instantiate(heartTemplate, heartTemplate.transform.parent);
            spawnedLive.SetActive(true);
            spawnedLivesUI.Add(spawnedLive);
        }
    }

    public void LooseLive()
    {
        if (spawnedLivesUI.Count > 0)
        {
            GameObject liveUI = spawnedLivesUI[^1];
            spawnedLivesUI.Remove(liveUI);
            Destroy(liveUI);
        }
    }

    public void GainLive() 
    {
        GameObject spawnedLive = Instantiate(heartTemplate, heartTemplate.transform.parent);
        spawnedLive.SetActive(true);
        spawnedLivesUI.Add(spawnedLive);
    }

    public void ClickedAnvil()
    {
        if (!HasOpenAnvil)
        {
            OpenCraftingUI();
        }
    }

    public void ClickUpgradeButton(UpgradeButton upBtn)
    {
        ChooseUpgrade(upBtn.StoredUpgrade);
        upgradeScreen.gameObject.SetActive(false);
        Time.timeScale = 1f;
        PlayerController.instance.enabled = true;
        DwarfController.instance.enabled = true;
        HasOpenAnvil = false;
    }

    private void ChooseUpgrade(Upgrade upgrade)
    {
        DwarfGameManager.instance.ApplyUpgrade(upgrade);
    }

    public void HoverUpgradeButton(UpgradeButton upBtn)
    {
        explainText.text = upBtn.StoredUpgrade.Description;
    }

    public void StopHoverButton()
    {
        explainText.text = "";
    }

    private void OpenCraftingUI()
    {
        HasOpenAnvil = true;
        DwarfController.instance.enabled = false;    
        PlayerController.instance.enabled = false;
        Time.timeScale = 0f;

        List<Upgrade> availableUpgrades = new(DwarfGameManager.instance.upgradePool);
        availableUpgrades.RemoveAll(u => DwarfGameManager.instance.AppliedUpgrades.Contains(u));

        upgrade1btn.gameObject.SetActive(false); upgrade2btn.gameObject.SetActive(false); upgrade3btn.gameObject.SetActive(false);

        Upgrade rdnUpgrade = null;

        if (availableUpgrades.Count > 0)
        {
            rdnUpgrade = availableUpgrades[UnityEngine.Random.Range(0, availableUpgrades.Count)];
            upgrade1btn.SetUpgrade(rdnUpgrade);
            upgrade1btn.gameObject.SetActive(true);
            availableUpgrades.Remove(rdnUpgrade);
        }

        if (availableUpgrades.Count > 0)
        {
            rdnUpgrade = availableUpgrades[UnityEngine.Random.Range(0, availableUpgrades.Count)];
            upgrade2btn.SetUpgrade(rdnUpgrade);
            upgrade2btn.gameObject.SetActive(true);
            availableUpgrades.Remove(rdnUpgrade);
        }

        if (availableUpgrades.Count > 0)
        {
            rdnUpgrade = availableUpgrades[UnityEngine.Random.Range(0, availableUpgrades.Count)];
            upgrade3btn.SetUpgrade(rdnUpgrade);
            upgrade3btn.gameObject.SetActive(true);
            availableUpgrades.Remove(rdnUpgrade);
        }

        upgradeScreen.gameObject.SetActive(true);
    }

}
