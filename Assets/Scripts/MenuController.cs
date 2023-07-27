using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using static Cinemachine.DocumentationSortingAttribute;

public class MenuController : MonoBehaviour
{
    public static MenuController instance;

    public RectTransform introScreen;
    public RectTransform mainGameScreen;
    public CanvasGroup fadeToBlack;
    public float fadeSpeed = 1f;

    // Main UI
    [SerializeField] private RectTransform livesRect;
    [SerializeField] private GameObject heartTemplate;
    [SerializeField] private GameObject armourTemplate;

    private List<GameObject> spawnedLivesUI = new();
    private List<GameObject> spawnedArmourUI = new();

    private bool startedGame = false;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        DontDestroyOnLoad(gameObject);
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

    private void LoadFirstScene()
    {
        introScreen.gameObject.SetActive(false);
        StartCoroutine(LoadFirstSceneAsync());
    }

    IEnumerator LoadFirstSceneAsync()
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("Main");

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
}
