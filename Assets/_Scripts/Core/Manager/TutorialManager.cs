using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;
using DG.Tweening;

public class TutorialManager : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private TutorialSequenceSO tutorialSequence;

    [Header("UI References")]
    [SerializeField] private GameObject tutorialRootPanel;   // Panel induk tutorial di StageScene
    [SerializeField] private Image displaySlideImage;         // Image gambar panduan yang sudah di-highlight
    [SerializeField] private RectTransform dialogBoxRect;    // RectTransform kotak dialog
    [SerializeField] private TMP_Text dialogText;            // Teks dialog
    [SerializeField] private CharacterAvatarUI characterAvatar;

    [Header("Dialog Placement Anchors")]
    [SerializeField] private RectTransform[] dialogPositions;

    private int currentStepIndex = -1;
    private bool isTutorialActive = false;
    private float inputCooldown = 0f;

    public bool IsTutorialActive => isTutorialActive;

    private void Start()
    {
        if (tutorialRootPanel != null)
            tutorialRootPanel.SetActive(false);

        // Cek apakah masuk ke StageScene dari StartNewRun (Main Menu)
        if (GameManager.Instance != null && GameManager.Instance.shouldShowTutorial)
        {
            GameManager.Instance.shouldShowTutorial = false; // Reset agar tidak muncul saat dari Shop -> Stage
            StartTutorial();
        }
    }

    private void Update()
    {
        if (!isTutorialActive) return;

        if (inputCooldown > 0f)
        {
            inputCooldown -= Time.unscaledDeltaTime;
            return;
        }

        // New Input System: Klik kiri mouse (atau tap touchscreen) di mana saja untuk lanjut
        bool isLeftClickPressed = Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
        bool isTouchPressed = Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame;

        if (isLeftClickPressed || isTouchPressed)
        {
            NextStep();
        }
    }

    public void StartTutorial()
    {
        if (tutorialSequence == null || tutorialSequence.steps.Count == 0)
        {
            FinishTutorial();
            return;
        }

        isTutorialActive = true;
        currentStepIndex = -1;

        // Kunci klik papan kartu selama tutorial berjalan
        if (MatchManager.Instance != null)
            MatchManager.Instance.SetInputLocked(true);

        if (tutorialRootPanel != null)
            tutorialRootPanel.SetActive(true);

        NextStep();
    }

    public void NextStep()
    {
        currentStepIndex++;

        if (currentStepIndex >= tutorialSequence.steps.Count)
        {
            FinishTutorial();
            return;
        }

        inputCooldown = 0.15f;
        TutorialStepData step = tutorialSequence.steps[currentStepIndex];

        // 1. Ganti gambar panduan
        if (displaySlideImage != null && step.slideImage != null)
        {
            displaySlideImage.sprite = step.slideImage;
        }

        // 2. Pindahkan posisi kotak dialog sesuai array dialogPositions
        if (dialogBoxRect != null && dialogPositions != null && dialogPositions.Length > 0)
        {
            int clampedIndex = Mathf.Clamp(step.dialogPositionIndex, 0, dialogPositions.Length - 1);
            if (dialogPositions[clampedIndex] != null)
            {
                dialogBoxRect.position = dialogPositions[clampedIndex].position;
            }

            dialogBoxRect.DOKill(true);
            dialogBoxRect.localScale = Vector3.one * 0.94f;
            dialogBoxRect.DOScale(1f, 0.2f).SetEase(Ease.OutBack);
        }

        // 3. Tampilkan teks langsung
        if (dialogText != null)
        {
            dialogText.text = step.dialogText;
        }

        // 4. Ubah pose karakter
        if (characterAvatar != null)
        {
            characterAvatar.SetPose(step.characterPose, animatePunch: true);
        }
    }

    private void FinishTutorial()
    {
        isTutorialActive = false;

        if (tutorialRootPanel != null)
            tutorialRootPanel.SetActive(false);

        if (characterAvatar != null)
            characterAvatar.SetPose(AvatarPose.Idle, animatePunch: true);

        // Buka kunci papan agar pemain bisa langsung mengklik kartu
        if (MatchManager.Instance != null)
            MatchManager.Instance.SetInputLocked(false);
    }
}