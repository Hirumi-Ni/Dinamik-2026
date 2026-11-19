using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;

public class CardView : MonoBehaviour
{
    [Header("Visual & Shadow References")]
    [SerializeField] private RectTransform visualContainer;
    [SerializeField] private RectTransform shadowTransform;
    [SerializeField] private Image shadowImage;
    [SerializeField] private Image cardBackgroundImage;
    [SerializeField] private Image cardIconImage;
    [SerializeField] private Sprite cardFrontViewBG;
    [SerializeField] private Sprite cardBackViewBG;
    [SerializeField] private CanvasGroup cardCanvasGroup;

    [Header("Bomb Warning")]
    [SerializeField] private GameObject bombWarningOverlay;

    [Header("DOTween & Hover Tilt Settings")]
    [SerializeField] private float wobbleAngle = 2.5f;
    [SerializeField] private float wobbleDuration = 1.6f;
    [SerializeField] private float hoverScale = 1.08f;
    [SerializeField] private float hoverLiftY = 14f;
    [SerializeField] private float maxTiltAngle = 14f;     // Maksimal kemiringan 3D saat di-hover
    [SerializeField] private float tiltSmoothSpeed = 14f;  // Kehalusan interpolasi tilt
    [SerializeField] private Vector2 hoverShadowOffset = new Vector2(10f, -16f);
    [SerializeField] private float flipHalfDuration = 0.14f;

    private Coroutine peekCoroutine;
    private Tween idleWobbleTween;
    private Sequence hoverSequence;
    private Sequence flipSequence;
    private Tween dealTween;

    private RectTransform rootRectTransform;
    private Quaternion targetTiltRotation = Quaternion.identity;

    private bool isHovered = false;
    private bool isLockedState = false;

    private void Awake()
    {
        rootRectTransform = GetComponent<RectTransform>();
    }

    private void Update()
    {
        // Terapkan rotasi tilt secara halus hanya saat kartu sedang di-hover
        if (isHovered && !isLockedState && visualContainer != null)
        {
            visualContainer.localRotation = Quaternion.Slerp(
                visualContainer.localRotation,
                targetTiltRotation,
                Time.deltaTime * tiltSmoothSpeed
            );
        }
    }

    private void OnDestroy()
    {
        KillAllTweens();
    }

    public void KillAllTweens()
    {
        idleWobbleTween?.Kill();
        hoverSequence?.Kill();
        flipSequence?.Kill();
        dealTween?.Kill();

        if (peekCoroutine != null)
        {
            StopCoroutine(peekCoroutine);
            peekCoroutine = null;
        }
    }

    public void InitializeVisual(Sprite iconSprite)
    {
        KillAllTweens();

        isHovered = false;
        isLockedState = false;
        targetTiltRotation = Quaternion.identity;

        if (cardCanvasGroup != null) cardCanvasGroup.alpha = 1f;
        if (bombWarningOverlay != null) bombWarningOverlay.SetActive(false);

        if (visualContainer != null)
        {
            visualContainer.anchoredPosition = Vector2.zero;
            visualContainer.localScale = Vector3.one;
            visualContainer.localRotation = Quaternion.identity;
        }

        ResetShadowImmediate();

        cardBackgroundImage.color = Color.white;
        cardIconImage.sprite = iconSprite;

        ShowBack();
        StartIdleWobble();
    }

    #region [1. Static Idle Wobble]
    public void StartIdleWobble()
    {
        if (visualContainer == null || isLockedState) return;

        idleWobbleTween?.Kill();

        float randomSign = (Random.value > 0.5f) ? 1f : -1f;
        visualContainer.localRotation = Quaternion.Euler(0f, 0f, -wobbleAngle * randomSign);

        idleWobbleTween = visualContainer
            .DOLocalRotate(new Vector3(0f, 0f, wobbleAngle * randomSign), wobbleDuration)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);

        idleWobbleTween.Goto(Random.Range(0f, wobbleDuration), true);
    }
    #endregion

    #region [2. Hover Lift, 3D Tilt & Shadow]
    public void PlayHoverEnter()
    {
        if (isLockedState || visualContainer == null) return;
        isHovered = true;

        idleWobbleTween?.Pause();
        hoverSequence?.Kill();

        hoverSequence = DOTween.Sequence();
        hoverSequence.Join(visualContainer.DOScale(hoverScale, 0.16f).SetEase(Ease.OutBack));
        hoverSequence.Join(visualContainer.DOAnchorPosY(hoverLiftY, 0.16f).SetEase(Ease.OutQuad));

        if (shadowTransform != null)
            hoverSequence.Join(shadowTransform.DOAnchorPos(hoverShadowOffset, 0.16f).SetEase(Ease.OutQuad));
        if (shadowImage != null)
            hoverSequence.Join(shadowImage.DOFade(0.45f, 0.16f));
    }

    public void UpdateHoverTilt(PointerEventData eventData)
    {
        if (!isHovered || isLockedState || rootRectTransform == null) return;

        // Ubah posisi layar kursor menjadi koordinat lokal di dalam kotak kartu
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rootRectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 localPoint))
        {
            Vector2 halfSize = rootRectTransform.rect.size * 0.5f;
            if (halfSize.x <= 0f || halfSize.y <= 0f) return;

            // Normalisasi posisi kursor (-1 di tepi kiri/bawah sampai +1 di tepi kanan/atas)
            float normX = Mathf.Clamp(localPoint.x / halfSize.x, -1f, 1f);
            float normY = Mathf.Clamp(localPoint.y / halfSize.y, -1f, 1f);

            // Miringkan kartu menghadap ke arah kursor
            float tiltX = normY * maxTiltAngle;
            float tiltY = -normX * maxTiltAngle;
            float tiltZ = -normX * (maxTiltAngle * 0.25f); // Sedikit rotasi Z agar terasa lebih organik

            targetTiltRotation = Quaternion.Euler(tiltX, tiltY, tiltZ);

            // Geser bayangan sedikit berlawanan arah kemiringan untuk efek kedalaman 3D
            if (shadowTransform != null)
            {
                Vector2 dynamicShadow = hoverShadowOffset + new Vector2(-normX * 6f, -normY * 6f);
                shadowTransform.anchoredPosition = dynamicShadow;
            }
        }
    }

    public void PlayHoverExit()
    {
        if (!isHovered) return;
        isHovered = false;
        targetTiltRotation = Quaternion.identity;

        if (visualContainer == null) return;

        hoverSequence?.Kill();
        hoverSequence = DOTween.Sequence();
        hoverSequence.Join(visualContainer.DOScale(1f, 0.15f).SetEase(Ease.OutQuad));
        hoverSequence.Join(visualContainer.DOAnchorPos(Vector2.zero, 0.15f).SetEase(Ease.OutQuad));
        hoverSequence.Join(visualContainer.DOLocalRotate(Vector3.zero, 0.15f).SetEase(Ease.OutQuad));

        if (shadowTransform != null)
            hoverSequence.Join(shadowTransform.DOAnchorPos(Vector2.zero, 0.15f).SetEase(Ease.OutQuad));
        if (shadowImage != null)
            hoverSequence.Join(shadowImage.DOFade(0.18f, 0.15f));

        hoverSequence.OnComplete(() =>
        {
            if (!isLockedState && !isHovered) StartIdleWobble();
        });
    }

    private void ResetShadowImmediate()
    {
        if (shadowTransform != null) shadowTransform.anchoredPosition = Vector2.zero;
        if (shadowImage != null)
        {
            Color c = shadowImage.color;
            c.a = 0.18f;
            shadowImage.color = c;
        }
    }
    #endregion

    #region [3. Squeeze & Swap Flip]
    public void PlayFlip(bool showFrontSide)
    {
        if (peekCoroutine != null)
        {
            StopCoroutine(peekCoroutine);
            peekCoroutine = null;
        }

        if (cardCanvasGroup != null) cardCanvasGroup.alpha = 1f;

        if (visualContainer == null)
        {
            if (showFrontSide) ShowFront();
            else ShowBack();
            return;
        }

        idleWobbleTween?.Pause();
        flipSequence?.Kill();

        float targetScale = isHovered ? hoverScale : 1f;

        flipSequence = DOTween.Sequence();
        flipSequence.Append(visualContainer.DOScale(new Vector3(0f, targetScale * 1.08f, 1f), flipHalfDuration).SetEase(Ease.InQuad));
        flipSequence.AppendCallback(() =>
        {
            if (showFrontSide) ShowFront();
            else ShowBack();
        });
        flipSequence.Append(visualContainer.DOScale(new Vector3(targetScale, targetScale, 1f), flipHalfDuration + 0.04f).SetEase(Ease.OutBack));
        flipSequence.OnComplete(() =>
        {
            if (!isLockedState && !isHovered) StartIdleWobble();
        });
    }
    #endregion

    #region [4. Deal From Bottom Deck]
    public void PrepareHiddenForDeal()
    {
        if (visualContainer == null) return;
        idleWobbleTween?.Pause();
        visualContainer.localScale = Vector3.zero;
    }

    public void PlayDeal(Vector3 deckWorldOrigin, float delay, System.Action onDealComplete = null)
    {
        if (visualContainer == null)
        {
            onDealComplete?.Invoke();
            return;
        }

        idleWobbleTween?.Pause();
        dealTween?.Kill();

        // Tetap sembunyi (scale 0) sampai delay gilirannya tiba
        visualContainer.localScale = Vector3.zero;

        Sequence dealSeq = DOTween.Sequence();
        dealSeq.SetDelay(delay);

        // Tepat saat giliran kartu ini terbang, tempatkan di deckSpawnPoint dan munculkan!
        dealSeq.AppendCallback(() =>
        {
            visualContainer.position = deckWorldOrigin;
            visualContainer.localScale = Vector3.one * 0.65f;
            visualContainer.localRotation = Quaternion.Euler(0f, 0f, Random.Range(-15f, 15f));
        });

        dealSeq.Append(visualContainer.DOAnchorPos(Vector2.zero, 0.38f).SetEase(Ease.OutCubic));
        dealSeq.Join(visualContainer.DOScale(1f, 0.35f).SetEase(Ease.OutBack));
        dealSeq.Join(visualContainer.DOLocalRotate(Vector3.zero, 0.35f).SetEase(Ease.OutQuad));

        dealSeq.OnComplete(() =>
        {
            onDealComplete?.Invoke();
            if (!isLockedState && !isHovered) StartIdleWobble();
        });

        dealTween = dealSeq;
    }
    #endregion

    #region [Card State Visuals: Matched, Bomb, Peek, Garlic]
    public void SetMatchedVisual()
    {
        LockMotion(alpha: 0.5f);
        visualContainer?.DOPunchScale(Vector3.one * 0.12f, 0.22f, 5, 1f);
    }

    public void SetBombArmedVisual()
    {
        LockMotion(alpha: 1f);
        ShowFront();
        cardBackgroundImage.color = new Color(1f, 0.55f, 0f, 1f);
        visualContainer?.DOPunchScale(Vector3.one * 0.15f, 0.25f, 6, 1f);
    }

    public void SetBombExplodedVisual()
    {
        LockMotion(alpha: 1f);
        ShowFront();
        cardBackgroundImage.color = Color.red;
        visualContainer?.DOShakePosition(0.35f, 12f, 20);
    }

    public void SetBombDefusedVisual()
    {
        LockMotion(alpha: 0.5f);
        ShowFront();
        cardBackgroundImage.color = Color.gray;
    }

    private void LockMotion(float alpha)
    {
        isLockedState = true;
        isHovered = false;
        targetTiltRotation = Quaternion.identity;
        idleWobbleTween?.Kill();
        hoverSequence?.Kill();

        if (bombWarningOverlay != null) bombWarningOverlay.SetActive(false);
        if (cardCanvasGroup != null) cardCanvasGroup.alpha = alpha;

        if (visualContainer != null)
        {
            visualContainer.DOAnchorPos(Vector2.zero, 0.15f);
            visualContainer.DOScale(1f, 0.15f);
            visualContainer.DOLocalRotate(Vector3.zero, 0.15f);
        }
        ResetShadowImmediate();
    }

    public void PlayPeek(float duration, System.Func<bool> canRevertToBack)
    {
        if (peekCoroutine != null) StopCoroutine(peekCoroutine);
        peekCoroutine = StartCoroutine(PeekRoutine(duration, canRevertToBack));
    }

    private IEnumerator PeekRoutine(float duration, System.Func<bool> canRevertToBack)
    {
        ShowFront();
        if (cardCanvasGroup != null) cardCanvasGroup.alpha = 0.45f;

        yield return new WaitForSeconds(duration);

        if (canRevertToBack != null && canRevertToBack())
        {
            if (cardCanvasGroup != null) cardCanvasGroup.alpha = 1f;
            ShowBack();
        }

        peekCoroutine = null;
    }

    public void ShowMarkedBombVisual()
    {
        if (peekCoroutine != null)
        {
            StopCoroutine(peekCoroutine);
            peekCoroutine = null;
        }

        ShowBack();
        cardBackgroundImage.color = new Color(0.65f, 0.15f, 0.15f, 1f);
        if (cardCanvasGroup != null) cardCanvasGroup.alpha = 1f;
        if (bombWarningOverlay != null) bombWarningOverlay.SetActive(true);
    }

    private void ShowFront()
    {
        cardIconImage.gameObject.SetActive(true);
        cardBackgroundImage.sprite = cardFrontViewBG;
    }

    private void ShowBack()
    {
        cardIconImage.gameObject.SetActive(false);
        cardBackgroundImage.sprite = cardBackViewBG;
    }
    #endregion
}