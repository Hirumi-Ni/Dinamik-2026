
using DG.Tweening;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class TooltipScript : MonoBehaviour
{
    [Header("Root & Text References")]
    [SerializeField] private GameObject tooltipVisualRoot;
    [SerializeField] private TMP_Text headerField;
    [SerializeField] private TMP_Text contentField;
    private CanvasGroup canvasGroup;

    private Tween fadeTween;

    [Header("Dynamic Sizing")]
    [SerializeField] private LayoutElement layoutElement;
    [SerializeField] private int characterWrapLimit = 35;
    [SerializeField] private RectTransform rectTransform;

    #region [Color Text]
    private const string COLOR_TOKEN = "#FFD700";
    private const string COLOR_MULT = "#E0486D";
    private const string COLOR_SCORE = "#4FA3E3"; 
    private const string COLOR_HP = "#2E7D32"; 
    private const string COLOR_TEMPO = "#D97706"; 
    private const string COLOR_SIGIL = "#FFFFFF"; 
    private const string COLOR_DANGER = "#C62828"; 
    #endregion

    private void Awake()
    {
        canvasGroup = tooltipVisualRoot.GetComponent<CanvasGroup>();

        GameEventHandler.OnItemTooltipShow += Show;
        GameEventHandler.OnItemTooltipHide += Hide;

        canvasGroup.alpha = 0f;
        tooltipVisualRoot.SetActive(false);
    }

    private void OnDestroy()
    {
        GameEventHandler.OnItemTooltipShow -= Show;
        GameEventHandler.OnItemTooltipHide -= Hide;

        fadeTween?.Kill();
    }

    public void Show(ShopItemDataSO data)
    {
        if (data == null || tooltipVisualRoot == null) return;

        fadeTween?.Kill();

        string header = data.itemName;
        string content = (UpgradeManager.Instance != null) ? UpgradeManager.Instance.GetFormattedDescription(data) : data.description;

        SetText(content, header);

        tooltipVisualRoot.SetActive(true);
        canvasGroup.alpha = 0f;

        fadeTween = DOTween.Sequence().AppendInterval(0.4f).Append(canvasGroup.DOFade(1f, 0.2f).SetEase(Ease.OutQuad));
    }

    public void Hide()
    {
        if (tooltipVisualRoot == null || !tooltipVisualRoot.activeSelf) return;

        fadeTween?.Kill();

        fadeTween = DOTween.Sequence().AppendInterval(0f).Append(canvasGroup.DOFade(0f, 0.2f).SetEase(Ease.InQuad)).OnComplete(() =>
        {
            tooltipVisualRoot.SetActive(false);
        });
    }

    public void SetText(string content, string header = "")
    {
        if (headerField != null)
        {
            if (string.IsNullOrEmpty(header))
            {
                headerField.gameObject.SetActive(false);
            }
            else
            {
                headerField.gameObject.SetActive(true);
                headerField.text = header;
            }
        }

        if (contentField != null) contentField.text = ApplyKeywordColors(content);

        if (layoutElement != null)
        {
            int headerLength = headerField != null ? headerField.text.Length : 0;
            int contentLength = contentField != null ? contentField.text.Length : 0;

            layoutElement.enabled = headerLength > characterWrapLimit || contentLength > characterWrapLimit;
        }
    }

    private void Update()
    {
        if (Mouse.current == null) return;

        Vector2 position = Mouse.current.position.ReadValue();

        var normalizedPosition = new Vector2(position.x / Screen.width, position.y / Screen.height);

        var pivot = CalculatePivot(normalizedPosition);

        rectTransform.pivot = pivot;
        tooltipVisualRoot.transform.position = position;
    }

    private Vector2 CalculatePivot(Vector2 normalizedPosition)
    {
        var pivotTopLeft = new Vector2(-0.05f, 1.05f);
        var pivotTopRight = new Vector2(1.05f, 1.05f);
        var pivotBottomLeft = new Vector2(-0.05f, -0.05f);
        var pivotBottomRight = new Vector2(1.05f, -0.05f);

        if (normalizedPosition.x < 0.5f && normalizedPosition.y >= 0.5f) return pivotTopLeft;
        if (normalizedPosition.x > 0.5f && normalizedPosition.y >= 0.5f) return pivotTopRight;
        if (normalizedPosition.x <= 0.5f && normalizedPosition.y < 0.5f) return pivotBottomLeft;

        return pivotBottomRight;
    }

    private string ApplyKeywordColors(string text)
    {
        if (string.IsNullOrEmpty(text)) return text;

        // Matches optional numbers/multipliers before the keyword (e.g., "+40% bonus Tokens", "+0.15x Base Mult", "+50 Base Score")
        text = Regex.Replace(text, @"(\+?\d+%?\s*(bonus\s+)?)?\bTokens?\b", $"<color={COLOR_TOKEN}>$0</color>");
        text = Regex.Replace(text, @"(\+?\d+(\.\d+)?x?\s*)?\b(Base\s+|Match\s+)?Mult\b", $"<color={COLOR_MULT}>$0</color>");
        text = Regex.Replace(text, @"(\+?\d+\s*)?\b(Base\s+)?Score\b", $"<color={COLOR_SCORE}>$0</color>");
        text = Regex.Replace(text, @"(\+?\d+\s*)?\b(Max\s+)?HP\b", $"<color={COLOR_HP}>$0</color>");
        text = Regex.Replace(text, @"(\+?\d+x?\+?\s*(bonus\s+)?)?\b(Turns?|Combo)\b|\bTurn\s+\d+\b", $"<color={COLOR_TEMPO}>$0</color>");
        text = Regex.Replace(text, @"\b([Aa]ll\s+Sigils|Triquetra|Crescent|Ankh|Lotus)\b", $"<color={COLOR_SIGIL}><u>$0</u></color>");
        text = Regex.Replace(text, @"\bBomb\b", $"<color={COLOR_DANGER}>$0</color>");

        return text;
    }
}
