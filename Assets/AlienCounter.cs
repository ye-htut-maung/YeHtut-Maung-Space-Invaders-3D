using UnityEngine;
using UnityEngine.Serialization;
using TMPro;

public class AlienCounter : MonoBehaviour
{
    [Header("Dynamic")]
    [FormerlySerializedAs("score")]
    [SerializeField] private int remainingAliens;
    public int RemainingAliens => remainingAliens;
    private TextMeshProUGUI uiText;
    private bool hasDisplayedCount;

    void Awake()
    {
        uiText = GetComponent<TextMeshProUGUI>();
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        transform.SetParent(GameOverController.EnsureExists().StatusRoot, false);
        RectTransform rect = uiText.rectTransform;
        rect.anchorMin = rect.anchorMax = new Vector2(0, 1);
        rect.pivot = new Vector2(0, 1);
        rect.anchoredPosition = new Vector2(28, -90);
        rect.sizeDelta = new Vector2(156, 48);
        rect.localScale = Vector3.one;
        uiText.fontSize = 24;
        uiText.fontStyle = FontStyles.Normal;
        uiText.color = Color.white;
        uiText.alignment = TextAlignmentOptions.TopLeft;
        uiText.richText = true;
        uiText.enableAutoSizing = false;
        uiText.textWrappingMode = TextWrappingModes.NoWrap;
        uiText.raycastTarget = false;
        SetRemainingAliens(remainingAliens);
    }

    public void SetRemainingAliens(int count)
    {
        count = Mathf.Max(0, count);
        if (hasDisplayedCount && remainingAliens == count)
        {
            return;
        }

        remainingAliens = count;
        hasDisplayedCount = true;
        uiText.text = "<size=11><color=#BE8CFF>ALIENS LEFT</color></size>\n<b>"
            + remainingAliens.ToString("00") + "</b>";
    }
}
