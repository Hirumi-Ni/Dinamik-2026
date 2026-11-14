using UnityEngine;

public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager Instance { get; private set; }
    public int turnsTakenInStage { get; private set; } = 0;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private GameRunStateData Run => GameManager.Instance.CurrentRun;

    public void ResetStageTracker()
    {
        turnsTakenInStage = 0;
        if (Run != null) Run.hasTriggeredUndyingThisStage = false;
    }

    public void RegisterTurnAttempt()
    {
        turnsTakenInStage++;
    }

    public int GetEffectiveBaseScore(CardDataSO card)
    {
        int score = card.cardBaseScore = Run.GetSymbolBonus(card.cardType);

        if (Run.HasSeal(WaxSealType.Crescendo)) score += Run.crescendoStack * 50;
        if (Run.HasSeal(WaxSealType.Ouroboros)) score += Run.crescendoStack * 20;

        return score;
    }

    public float GetEffectiveBaseMult(float turnMult, CardDataSO card, int remainingturn)
    {
        float baseMult = turnMult;

        if (Run.HasSeal(WaxSealType.GlassCannon))
        {
            int missingHP = Mathf.Max(0, Run.GetEffectiveMaxHP() - Run.currentHP);
            baseMult += missingHP * 0.75f;
        }

        if (Run.HasSeal(WaxSealType.TrinityCadence) && card.cardType == CardType.Triquetra && remainingturn % 3 == 0)
        {
            baseMult += 1.5f;
        }

        if (Run.HasSeal(WaxSealType.Ouroboros))
        {
            baseMult += Run.ouroborosStack * 0.15f;
        }

        return baseMult;
    }

    public float GetMatchMult(int currentCombo)
    {
        float mult = Mathf.Pow(2, currentCombo - 1);

        if (Run.HasSeal(WaxSealType.FirstBlood))
        {
            if (turnsTakenInStage == 0) mult *= 3.0f;
            else if (turnsTakenInStage == 1) mult *= 2.0f; 
        }

        return mult;
    }

    public void OnMatchResolved(CardDataSO card, int newCombo)
    {
        if (Run.HasSeal(WaxSealType.Crescendo) && newCombo >= 3) Run.crescendoStack++;

        if (Run.HasSeal(WaxSealType.AnkhBlessing) && card.cardType == CardType.Ankh)
        {
            int bonusTokens = Run.currentTokens / 10;
            if (bonusTokens > 0) GameManager.Instance.AddTokens(bonusTokens);
        }

        if (Run.HasSeal(WaxSealType.SereneLotus) && card.cardType == CardType.Lotus)
        {
            if (Run.currentHP >= Run.GetEffectiveMaxHP()) ScoreManager.Instance.AddTurn(1);
        }
    }

    public void OnClusterCleared()
    {
        if (Run.HasSeal(WaxSealType.Ouroboros)) Run.ouroborosStack++;
    }

    public int CalculateFinalStageTokens(int rawTokens, int clearedInCluster)
    {
        int total = rawTokens;

        if (Run.HasSeal(WaxSealType.SwiftExecution) && clearedInCluster == 1)
        {
            total += 30;
            GameManager.Instance.HealPlayer(1);
        }

        if (Run.HasSeal(WaxSealType.Fortune)) total = Mathf.CeilToInt(total * 1.4f);

        return total;
    }

    public string GetFormattedDescription(ShopItemDataSO item)
    {
        if (item == null) return string.Empty;
        if (item.type != ShopItemType.PassiveBuff || Run == null) return item.description;

        switch (item.sealType)
        {
            case WaxSealType.Crescendo:
                return $"{item.description}\n<color=#FFD700>(Currently: +{Run.crescendoStack * 50} Base Score)</color>";

            case WaxSealType.Ouroboros:
                return $"{item.description}\n<color=#FFD700>(Currently: +{Run.ouroborosStack * 20} Score, +{Run.ouroborosStack * 0.15f:0.00}x Mult)</color>";

            default:
                return item.description;
        }
    }
}
