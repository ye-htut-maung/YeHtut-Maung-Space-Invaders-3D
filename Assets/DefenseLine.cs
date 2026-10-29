using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DefenseLine : MonoBehaviour
{
    private AlienFleet fleet;
    private Camera gameCamera;
    private RectTransform lineRect;
    private RectTransform labelRect;
    private float gameplayDepth;

    public float Height { get; private set; }

    private void Start()
    {
        Spaceship ship = FindFirstObjectByType<Spaceship>();
        fleet = FindFirstObjectByType<AlienFleet>();
        gameCamera = Camera.main;
        if (ship == null || fleet == null || gameCamera == null)
        {
            enabled = false;
            return;
        }

        float shipTop = ship.transform.position.y;
        foreach (Renderer piece in ship.GetComponentsInChildren<Renderer>())
        {
            shipTop = Mathf.Max(shipTop, piece.bounds.max.y);
        }
        Height = shipTop + 0.5f;
        gameplayDepth = ship.transform.position.z;

        GameObject canvasObject = new GameObject("DefenseLineCanvas",
            typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
        canvasObject.transform.SetParent(transform, false);
        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;
        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280, 720);
        scaler.matchWidthOrHeight = 0.5f;

        GameObject line = new GameObject("DefenseLineVisual", typeof(RectTransform), typeof(Image));
        line.transform.SetParent(canvasObject.transform, false);
        lineRect = line.GetComponent<RectTransform>();
        lineRect.sizeDelta = new Vector2(-48, 2);
        Image image = line.GetComponent<Image>();
        image.color = new Color(1, 0.2f, 0.25f);
        image.raycastTarget = false;

        GameObject label = new GameObject("DefenseLineLabel",
            typeof(RectTransform), typeof(TextMeshProUGUI));
        label.transform.SetParent(canvasObject.transform, false);
        labelRect = label.GetComponent<RectTransform>();
        labelRect.pivot = new Vector2(1, 0);
        labelRect.sizeDelta = new Vector2(180, 26);
        TextMeshProUGUI text = label.GetComponent<TextMeshProUGUI>();
        text.text = "DEFENSE LINE";
        text.fontSize = 18;
        text.alignment = TextAlignmentOptions.Right;
        text.color = image.color;
        text.raycastTarget = false;
        UpdateVisual();
    }

    private void UpdateVisual()
    {
        float viewportY = gameCamera.WorldToViewportPoint(
            new Vector3(0, Height, gameplayDepth)).y;
        lineRect.anchorMin = new Vector2(0, viewportY);
        lineRect.anchorMax = new Vector2(1, viewportY);
        lineRect.anchoredPosition = Vector2.zero;
        labelRect.anchorMin = labelRect.anchorMax = new Vector2(1, viewportY);
        labelRect.anchoredPosition = new Vector2(-24, 6);
    }

    private void LateUpdate()
    {
        if (gameCamera == null || fleet == null)
        {
            return;
        }

        UpdateVisual();
        if (!GameOverController.IsPlaying)
        {
            return;
        }

        // Check after fleet movement, using only aliens still in the living fleet.
        foreach (GameObject alien in fleet.aliens)
        {
            if (alien == null || !alien.activeInHierarchy)
            {
                continue;
            }

            Renderer alienRenderer = alien.GetComponentInChildren<Renderer>();
            float bottom = alienRenderer != null
                ? alienRenderer.bounds.min.y
                : alien.transform.position.y;
            if (bottom <= Height)
            {
                GameOverController.EnsureExists().EndGame("Aliens invaded!");
                return;
            }
        }
    }
}
