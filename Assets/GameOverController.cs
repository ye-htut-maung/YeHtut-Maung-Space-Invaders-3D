using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOverController : MonoBehaviour
{
    public enum GameState { MainMenu, Playing, Paused, Won, Lost }

    private static GameOverController instance;
    private static GameState nextSceneState = GameState.MainMenu;
    private GameObject mainMenuPanel;
    private GameObject pausePanel;
    private GameObject resultPanel;
    private GameObject menuButton;
    private TextMeshProUGUI titleText;
    private TextMeshProUGUI reasonText;
    private HeartIcon[] hearts = new HeartIcon[Spaceship.StartingLives];

    public GameState State { get; private set; }
    public Transform HudRoot { get; private set; }
    public Transform StatusRoot { get; private set; }
    public static bool IsPlaying => instance != null && instance.State == GameState.Playing;
    public static bool IsGameOver => instance != null &&
        (instance.State == GameState.Won || instance.State == GameState.Lost);

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetSession()
    {
        instance = null;
        nextSceneState = GameState.MainMenu;
    }

    public static GameOverController EnsureExists()
    {
        if (instance == null)
        {
            instance = FindFirstObjectByType<GameOverController>();
            if (instance == null)
            {
                instance = new GameObject("GameOverController").AddComponent<GameOverController>();
            }
        }
        return instance;
    }

    private void Awake()
    {
        instance = this;
        ConfigurePlayfield();
        gameObject.AddComponent<GameBackground>();
        GameState initialState = nextSceneState;
        nextSceneState = GameState.MainMenu;

        GameObject canvasObject = new GameObject("GameMenuCanvas",
            typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasObject.transform.SetParent(transform, false);
        HudRoot = canvasObject.transform;
        canvasObject.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObject.GetComponent<Canvas>().sortingOrder = 100;
        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280, 720);
        scaler.matchWidthOrHeight = 0.5f;
        CreateLivesDisplay(canvasObject.transform);

        mainMenuPanel = CreatePanel("MainMenuPanel", canvasObject.transform);
        CreateText("GameTitle", "SPACE INVADERS 3D", mainMenuPanel.transform,
            new Vector2(0, 140), new Vector2(760, 90), 50);
        CreateText("Instructions", "Arrow keys to move  |  Space to shoot\n\nDefend the red line!",
            mainMenuPanel.transform, new Vector2(0, 25), new Vector2(600, 120), 24);
        CreateButton("StartButton", "Start Game", mainMenuPanel.transform,
            new Vector2(0, -95), StartGame);

        pausePanel = CreatePanel("PausePanel", canvasObject.transform);
        CreateText("PauseTitle", "PAUSED", pausePanel.transform,
            new Vector2(0, 105), new Vector2(480, 90), 56);
        CreateButton("ResumeButton", "Resume", pausePanel.transform, Vector2.zero, Resume);
        CreateButton("PauseMainMenuButton", "Main Menu", pausePanel.transform,
            new Vector2(0, -85), ReturnToMainMenu);

        resultPanel = CreatePanel("GameOverPanel", canvasObject.transform);
        titleText = CreateText("Title", "GAME OVER", resultPanel.transform,
            new Vector2(0, 125), new Vector2(480, 90), 56);
        reasonText = CreateText("Reason", "", resultPanel.transform,
            new Vector2(0, 55), new Vector2(480, 40), 24);
        CreateButton("RestartButton", "Restart", resultPanel.transform,
            new Vector2(0, -35), Restart);
        CreateButton("ResultMainMenuButton", "Main Menu", resultPanel.transform,
            new Vector2(0, -115), ReturnToMainMenu);

        menuButton = CreateButton("MenuButton", "Menu", canvasObject.transform, Vector2.zero, Pause);
        RectTransform menuRect = menuButton.GetComponent<RectTransform>();
        menuRect.anchorMin = menuRect.anchorMax = Vector2.one;
        menuRect.pivot = Vector2.one;
        menuRect.anchoredPosition = new Vector2(-24, -24);
        menuRect.sizeDelta = new Vector2(140, 48);

        SetState(initialState);
        gameObject.AddComponent<DefenseLine>();
    }

    private void CreateLivesDisplay(Transform parent)
    {
        GameObject panel = new GameObject("ShipStatusPanel", typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(parent, false);
        StatusRoot = panel.transform;
        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = panelRect.anchorMax = new Vector2(0, 1);
        panelRect.pivot = new Vector2(0, 1);
        panelRect.anchoredPosition = new Vector2(24, -24);
        panelRect.sizeDelta = new Vector2(200, 148);
        Image panelImage = panel.GetComponent<Image>();
        panelImage.color = new Color(0.015f, 0.035f, 0.075f, 0.15f);
        panelImage.raycastTarget = false;
        Color borderColor = new Color(0.2f, 0.85f, 1, 0.25f);
        CreateStatusAccent("LeftBorder", Vector2.zero, new Vector2(1, 148), borderColor);
        CreateStatusAccent("RightBorder", new Vector2(199, 0), new Vector2(1, 148), borderColor);
        CreateStatusAccent("BottomBorder", new Vector2(0, -147), new Vector2(200, 1), borderColor);
        CreateStatusAccent("CyanHeader", new Vector2(0, 0), new Vector2(200, 2),
            new Color(0.2f, 0.85f, 1));
        CreateStatusAccent("StatusDivider", new Vector2(16, -80), new Vector2(168, 1),
            new Color(0.2f, 0.85f, 1, 0.25f));
        CreateStatusAccent("VioletIndicator", new Vector2(16, -96), new Vector2(3, 34),
            new Color(0.75f, 0.35f, 1));

        TextMeshProUGUI label = CreateText("LivesLabel", "PILOT LIVES", StatusRoot,
            Vector2.zero, new Vector2(168, 20), 13);
        RectTransform labelRect = label.rectTransform;
        labelRect.anchorMin = labelRect.anchorMax = new Vector2(0, 1);
        labelRect.pivot = new Vector2(0, 1);
        labelRect.anchoredPosition = new Vector2(16, -12);
        label.alignment = TextAlignmentOptions.Left;
        label.characterSpacing = 2;
        label.color = new Color(0.65f, 0.92f, 1);
        for (int i = 0; i < hearts.Length; i++)
        {
            GameObject heart = new GameObject("Heart" + (i + 1),
                typeof(RectTransform), typeof(CanvasRenderer), typeof(HeartIcon));
            heart.transform.SetParent(StatusRoot, false);
            RectTransform rect = heart.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0, 1);
            rect.pivot = new Vector2(0, 1);
            rect.anchoredPosition = new Vector2(16 + i * 34, -38);
            rect.sizeDelta = new Vector2(24, 22);
            hearts[i] = heart.GetComponent<HeartIcon>();
            hearts[i].raycastTarget = false;
            heart.AddComponent<Outline>().effectDistance = new Vector2(1, -1);
        }
        UpdateLives(Spaceship.StartingLives);
    }

    private void CreateStatusAccent(string objectName, Vector2 position, Vector2 size, Color color)
    {
        GameObject accent = new GameObject(objectName, typeof(RectTransform), typeof(Image));
        accent.transform.SetParent(StatusRoot, false);
        RectTransform rect = accent.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = new Vector2(0, 1);
        rect.pivot = new Vector2(0, 1);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        Image image = accent.GetComponent<Image>();
        image.color = color;
        image.raycastTarget = false;
    }

    public void UpdateLives(int lives)
    {
        for (int i = 0; i < hearts.Length; i++)
        {
            hearts[i].color = i < lives
                ? new Color(0.2f, 0.85f, 1)
                : new Color(0.12f, 0.2f, 0.3f);
            hearts[i].GetComponent<Outline>().effectColor = i < lives
                ? new Color(0.2f, 0.85f, 1, 0.5f)
                : new Color(0.25f, 0.35f, 0.5f, 0.4f);
        }
    }

    private void ConfigurePlayfield()
    {
        Camera gameCamera = Camera.main;
        if (gameCamera == null || !gameCamera.orthographic) return;

        // Add space above the fleet while preserving the existing bottom edge.
        float bottomEdge = gameCamera.transform.position.y - gameCamera.orthographicSize;
        gameCamera.orthographicSize = 12f;
        Vector3 position = gameCamera.transform.position;
        position.y = bottomEdge + gameCamera.orthographicSize;
        gameCamera.transform.position = position;
    }

    private GameObject CreatePanel(string objectName, Transform parent)
    {
        GameObject panel = new GameObject(objectName, typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(parent, false);
        RectTransform rect = panel.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = rect.offsetMax = Vector2.zero;
        panel.GetComponent<Image>().color = new Color(0.02f, 0.03f, 0.08f, 0.94f);
        return panel;
    }

    private GameObject CreateButton(string objectName, string text, Transform parent,
        Vector2 position, UnityEngine.Events.UnityAction action)
    {
        GameObject buttonObject = new GameObject(objectName,
            typeof(RectTransform), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(parent, false);
        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(240, 64);
        Image image = buttonObject.GetComponent<Image>();
        image.color = new Color(0.12f, 0.4f, 0.65f);
        Button button = buttonObject.GetComponent<Button>();
        button.targetGraphic = image;
        // Keep Space reserved for shooting rather than submitting a focused UI button.
        button.navigation = new Navigation { mode = Navigation.Mode.None };
        button.onClick.AddListener(action);
        CreateText("Label", text, buttonObject.transform, Vector2.zero, rect.sizeDelta, 26);
        return buttonObject;
    }

    private TextMeshProUGUI CreateText(string objectName, string text, Transform parent,
        Vector2 position, Vector2 size, float fontSize)
    {
        GameObject textObject = new GameObject(objectName,
            typeof(RectTransform), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(parent, false);
        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        TextMeshProUGUI label = textObject.GetComponent<TextMeshProUGUI>();
        label.text = text;
        label.fontSize = fontSize;
        label.alignment = TextAlignmentOptions.Center;
        label.color = Color.white;
        label.raycastTarget = false;
        return label;
    }

    private void SetState(GameState state)
    {
        State = state;
        mainMenuPanel.SetActive(state == GameState.MainMenu);
        pausePanel.SetActive(state == GameState.Paused);
        resultPanel.SetActive(state == GameState.Won || state == GameState.Lost);
        menuButton.SetActive(state == GameState.Playing);
        Time.timeScale = state == GameState.Playing ? 1 : 0;
    }

    public void StartGame()
    {
        if (State == GameState.MainMenu) SetState(GameState.Playing);
    }

    public void Pause()
    {
        if (State == GameState.Playing) SetState(GameState.Paused);
    }

    public void Resume()
    {
        if (State == GameState.Paused) SetState(GameState.Playing);
    }

    public void EndGame(string reason = "Spaceship destroyed!")
    {
        ShowResult(GameState.Lost, "GAME OVER", reason);
    }

    public void WinGame()
    {
        ShowResult(GameState.Won, "YOU WIN!", "All aliens destroyed!");
    }

    private void ShowResult(GameState state, string title, string reason)
    {
        if (State != GameState.Playing) return;
        titleText.text = title;
        reasonText.text = reason;
        SetState(state);
    }

    public void Restart()
    {
        if (State == GameState.Won || State == GameState.Lost)
            ReloadScene(GameState.Playing);
    }

    public void ReturnToMainMenu()
    {
        if (State == GameState.Paused || State == GameState.Won || State == GameState.Lost)
            ReloadScene(GameState.MainMenu);
    }

    private void ReloadScene(GameState initialState)
    {
        // Only the requested starting state survives the reload; round objects do not.
        nextSceneState = initialState;
        Time.timeScale = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().path);
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
            Time.timeScale = 1;
        }
    }
}
