using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class WaxSealHUDItem : MonoBehaviour
{
    [Header("UI Reference")]
    [SerializeField] private Image sealIconImage;

    [Header("Trigger Punch Settings")]
    [SerializeField] private Color triggerFlashColor = new Color(1f, 0.9f, 0.45f, 1f);
    [SerializeField] private float punchScaleAmount = 0.28f;

    private WaxSealType currentSealType;
    private bool hasSealEquipped = false;
    private Color defaultIconColor = Color.white;
    private RectTransform iconRect;
    private float lastTriggerTime = -1f;

    private void Awake()
    {
        if (sealIconImage != null)
        {
            iconRect = sealIconImage.rectTransform;
            defaultIconColor = sealIconImage.color;
        }
    }

    private void OnEnable()
    {
        GameEventHandler.OnWaxSealTriggered += HandleSealTriggered;
    }

    private void OnDisable()
    {
        GameEventHandler.OnWaxSealTriggered -= HandleSealTriggered;
        iconRect?.DOKill();
        sealIconImage?.DOKill();
    }

    public void Setup(ShopItemDataSO sealData)
    {
        if (sealData == null)
        {
            ClearSlot();
            return;
        }

        currentSealType = sealData.sealType;
        hasSealEquipped = true;

        if (sealIconImage != null)
        {
            if (iconRect == null) iconRect = sealIconImage.rectTransform;

            sealIconImage.gameObject.SetActive(true);
            sealIconImage.sprite = sealData.icon;
            sealIconImage.color = Color.white;
            defaultIconColor = Color.white;

            iconRect.localScale = Vector3.one;
            iconRect.localRotation = Quaternion.identity;
        }
    }

    public void ClearSlot()
    {
        hasSealEquipped = false;
        if (sealIconImage != null)
        {
            sealIconImage.sprite = null;
            sealIconImage.gameObject.SetActive(false);
        }
    }

    private void HandleSealTriggered(WaxSealType triggeredType)
    {
        if (!hasSealEquipped || triggeredType != currentSealType) return;
        PlayTriggerJuice();
    }

    public void PlayTriggerJuice()
    {
        if (Time.unscaledTime - lastTriggerTime < 0.15f) return;
        lastTriggerTime = Time.unscaledTime;

        //AudioManager.Instance.PlayAudio(); 

        if (iconRect != null)
        {
            iconRect.DOKill(complete: true);
            iconRect.localScale = Vector3.one;
            iconRect.localRotation = Quaternion.identity;

            iconRect.DOPunchScale(Vector3.one * punchScaleAmount, 0.35f, 6, 0.8f);
            iconRect.DOPunchRotation(new Vector3(0f, 0f, 12f), 0.35f, 8, 0.8f);
        }

        if (sealIconImage != null)
        {
            sealIconImage.DOKill();
            sealIconImage.color = triggerFlashColor;
            sealIconImage.DOColor(defaultIconColor, 0.45f).SetEase(Ease.OutQuad);
        }
    }
}