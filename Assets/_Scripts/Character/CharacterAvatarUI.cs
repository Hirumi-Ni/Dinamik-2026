using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public enum AvatarPose
{
    Idle,
    Idle2,
    Happy,
    Smug,
    Explaining
}

public class CharacterAvatarUI : MonoBehaviour
{
    [Header("UI Reference")]
    [SerializeField] private Image avatarImage;
    [SerializeField] private RectTransform avatarRect;

    [Header("Character Sprites")]
    [SerializeField] private Sprite idleSprite;
    [SerializeField] private Sprite idle2Sprite;
    [SerializeField] private Sprite happySprite;
    [SerializeField] private Sprite smugSprite;
    [SerializeField] private Sprite explainingSprite;

    [Header("Timing & Idle Settings")]
    [SerializeField] private float reactionDuration = 1.6f;
    [SerializeField] private float idlePoseSwitchInterval = 4.5f; 

    [Header("DOTween Settings")]
    [SerializeField] private float breatheYOffset = 6f;
    [SerializeField] private float breatheDuration = 1.8f;

    private Coroutine revertToIdleCoroutine;
    private Coroutine idleSwitchCoroutine;
    private Tween breatheTween;
    private Vector2 initialAnchoredPos;
    private AvatarPose currentPose = AvatarPose.Idle;
    private bool isReacting = false;
    private bool useSecondIdle = false;

    private void Awake()
    {
        if (avatarRect == null && avatarImage != null)
            avatarRect = avatarImage.GetComponent<RectTransform>();

        if (avatarRect != null)
            initialAnchoredPos = avatarRect.anchoredPosition;
    }

    private void OnEnable()
    {
        GameEventHandler.OnMatchSuccess += HandleMatchSuccess;
        GameEventHandler.OnMatchFailed += HandleMatchFailed;
        GameEventHandler.OnBombTriggered += HandleBombTriggered;
        GameEventHandler.OnGameStateChanged += HandleGameStateChanged;
    }

    private void OnDisable()
    {
        GameEventHandler.OnMatchSuccess -= HandleMatchSuccess;
        GameEventHandler.OnMatchFailed -= HandleMatchFailed;
        GameEventHandler.OnBombTriggered -= HandleBombTriggered;
        GameEventHandler.OnGameStateChanged -= HandleGameStateChanged;

        StopAllCoroutines();
        breatheTween?.Kill();
    }

    private void Start()
    {
        SetPose(AvatarPose.Idle, animatePunch: false);
        StartBreathingAnimation();
        idleSwitchCoroutine = StartCoroutine(IdlePoseSwitchRoutine());

        if (GameManager.Instance != null && GameManager.Instance.CurrentState == GameState.Shop)
        {
            TriggerReaction(AvatarPose.Explaining, 2.2f);
        }
    }

    #region [Event Listeners]
    private void HandleMatchSuccess(CardDataSO matchedCard, int currentCombo)
    {
        AvatarPose reaction = (currentCombo >= 3) ? AvatarPose.Idle2 : AvatarPose.Happy;
        TriggerReaction(reaction, reactionDuration);
    }

    private void HandleMatchFailed()
    {
        TriggerReaction(AvatarPose.Smug, 1.2f, shakeEffect: false);
    }

    private void HandleBombTriggered()
    {
        TriggerReaction(AvatarPose.Smug, 2.0f, shakeEffect: true);
    }

    private void HandleGameStateChanged(GameState newState)
    {
        if (newState == GameState.Shop)
        {
            TriggerReaction(AvatarPose.Explaining, 2.2f);
        }
        else if (newState == GameState.Match)
        {
            ReturnToCurrentIdle(animatePunch: true);
        }
    }
    #endregion

    #region [Pose & Animation Control]
    public void TriggerReaction(AvatarPose pose, float duration, bool shakeEffect = false)
    {
        isReacting = true;
        SetPose(pose, animatePunch: true, shakeEffect: shakeEffect);

        if (revertToIdleCoroutine != null)
            StopCoroutine(revertToIdleCoroutine);

        revertToIdleCoroutine = StartCoroutine(RevertToIdleAfterDelay(duration));
    }

    public void SetPose(AvatarPose newPose, bool animatePunch = true, bool shakeEffect = false)
    {
        currentPose = newPose;
        if (avatarImage == null) return;

        Sprite targetSprite = GetSpriteForPose(newPose);
        if (targetSprite != null)
        {
            avatarImage.sprite = targetSprite;
        }

        if (avatarRect == null) return;

        if (animatePunch)
        {
            avatarRect.DOKill(complete: true);
            avatarRect.anchoredPosition = initialAnchoredPos;
            avatarRect.localScale = Vector3.one;

            avatarRect.DOPunchScale(new Vector3(0.06f, -0.04f, 0f), 0.25f, 5, 0.8f);

            if (shakeEffect)
            {
                avatarRect.DOShakePosition(0.35f, new Vector3(10f, 4f, 0f), 18, 90f).OnComplete(() => avatarRect.anchoredPosition = initialAnchoredPos);
            }

            StartBreathingAnimation();
        }
    }

    private Sprite GetSpriteForPose(AvatarPose pose)
    {
        return pose switch
        {
            AvatarPose.Idle => idleSprite,
            AvatarPose.Idle2 => idle2Sprite != null ? idle2Sprite : idleSprite,
            AvatarPose.Happy => happySprite != null ? happySprite : idleSprite,
            AvatarPose.Smug => smugSprite != null ? smugSprite : idleSprite,
            AvatarPose.Explaining => explainingSprite != null ? explainingSprite : idleSprite,
            _ => idleSprite
        };
    }

    private void ReturnToCurrentIdle(bool animatePunch)
    {
        AvatarPose activeIdle = (useSecondIdle && idle2Sprite != null) ? AvatarPose.Idle2 : AvatarPose.Idle;
        SetPose(activeIdle, animatePunch: animatePunch);
    }

    private IEnumerator RevertToIdleAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        isReacting = false;
        revertToIdleCoroutine = null;
        ReturnToCurrentIdle(animatePunch: true);
    }

    private IEnumerator IdlePoseSwitchRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(idlePoseSwitchInterval + Random.Range(-0.8f, 1.2f));

            if (!isReacting && (currentPose == AvatarPose.Idle || currentPose == AvatarPose.Idle2) && idle2Sprite != null)
            {
                useSecondIdle = !useSecondIdle;
                ReturnToCurrentIdle(animatePunch: true);
            }
        }
    }

    private void StartBreathingAnimation()
    {
        if (avatarRect == null) return;

        breatheTween?.Kill();
        avatarRect.anchoredPosition = initialAnchoredPos;

        breatheTween = avatarRect.DOAnchorPosY(initialAnchoredPos.y + breatheYOffset, breatheDuration).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo);
    }
    #endregion
}