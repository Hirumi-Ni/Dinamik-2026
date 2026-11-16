using UnityEngine;
using TMPro;

public class SigilStatusUI : MonoBehaviour
{
    [Header("Card Data References")]
    [SerializeField] private CardDataSO triquetraData;
    [SerializeField] private CardDataSO crescentData;
    [SerializeField] private CardDataSO ankhData;
    [SerializeField] private CardDataSO lotusData;

    [Header("UI Value Texts")]
    [SerializeField] private TMP_Text triquetraValueText;
    [SerializeField] private TMP_Text crescentValueText;
    [SerializeField] private TMP_Text ankhValueText;
    [SerializeField] private TMP_Text lotusValueText;

    private void OnEnable() => GameEventHandler.OnSigilStatsUpdated += RefreshSigilValues;
    private void OnDisable() => GameEventHandler.OnSigilStatsUpdated -= RefreshSigilValues;

    private void Start() => RefreshSigilValues();

    public void RefreshSigilValues()
    {
        if (GameManager.Instance?.CurrentRun == null) return;
        GameRunStateData run = GameManager.Instance.CurrentRun;

        if (triquetraValueText != null && triquetraData != null) triquetraValueText.text = (triquetraData.cardBaseScore + run.GetSymbolBonus(CardType.Triquetra)).ToString();
        if (crescentValueText != null && crescentData != null) crescentValueText.text = (crescentData.cardBaseScore + run.GetSymbolBonus(CardType.Crescent)).ToString();
        if (ankhValueText != null && ankhData != null) ankhValueText.text = (ankhData.cardBaseScore + run.GetSymbolBonus(CardType.Ankh)).ToString();
        if (lotusValueText != null && lotusData != null) lotusValueText.text = (lotusData.cardBaseScore + run.GetSymbolBonus(CardType.Lotus)).ToString();
    }
}