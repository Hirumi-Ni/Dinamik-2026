using UnityEngine;

public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager Instance { get; private set; }

    public int turnsTakenInStage { get; private set; } = 0;
    public int elixirRemainingTurns { get; private set; } = 0;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }

    private GameRunStateData Run => GameManager.Instance.CurrentRun;

    public void ResetStageTracker()
    {
        turnsTakenInStage = 0;
        elixirRemainingTurns = 0;
        if (Run != null) Run.hasTriggeredUndyingThisStage = false;
    }

    public void RegisterTurnAttempt()
    {
        turnsTakenInStage++;

        if (elixirRemainingTurns > 0) elixirRemainingTurns--;
    }

    public int GetEffectiveBaseScore(CardDataSO card)
    {
        int score = card.cardBaseScore + Run.GetSymbolBonus(card.cardType);

        if (Run.HasSeal(WaxSealType.Crescendo)) score += Run.crescendoStack * 50;
        if (Run.HasSeal(WaxSealType.Ouroboros)) score += Run.ouroborosStack * 20;

        return score;
    }

    public float GetEffectiveBaseMult(float turnMult, CardDataSO card, int remainingTurn)
    {
        float baseMult = turnMult;

        if (Run.HasSeal(WaxSealType.GlassCannon))
        {
            int missingHP = Mathf.Max(0, Run.GetEffectiveMaxHP() - Run.currentHP);
            baseMult += missingHP * 0.75f;
        }

        if (Run.HasSeal(WaxSealType.TrinityCadence) && card.cardType == CardType.Triquetra && remainingTurn % 3 == 0)
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

        if (elixirRemainingTurns > 0)
        {
            mult *= 2.0f;
        }

        return mult;
    }

    public void ActivateConsumable(ShopItemDataSO item)
    {
        if (item == null) return;

        switch (item.consumableType)
        {
            case ConsumableType.Lens:
                if (BoardManager.Instance != null) BoardManager.Instance.PeekRandomCards(3, 1.5f);
                break;

            case ConsumableType.Hourglass:
                if (ScoreManager.Instance != null) ScoreManager.Instance.AddTurn(3);
                break;

            case ConsumableType.Garlic:
                if (MatchManager.Instance != null && MatchManager.Instance.TryDefuseActiveDeathCard())
                {
                    break;
                }

                if (BoardManager.Instance != null)
                {
                    BoardManager.Instance.MarkOrDisarmBomb();
                }
                break;

            case ConsumableType.Elixir:
                elixirRemainingTurns = 3;
                break;
        }
    }

    public void OnMatchResolved(CardDataSO card, int newCombo)
    {
        if (Run.HasSeal(WaxSealType.Crescendo) && newCombo >= 3) Run.crescendoStack++;

        if (Run.HasSeal(WaxSealType.AnkhBlessing) && card.cardType == CardType.Ankh)
        {
            int bonusTokens = Run.currentTokens / 15;
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
                int crescendoBonus = Run.crescendoStack * 50;
                return $"{item.description}\n(Currently: +{crescendoBonus} Base Score)";

            case WaxSealType.Ouroboros:
                int ouroScore = Run.ouroborosStack * 20;
                float ouroMult = Run.ouroborosStack * 0.15f;
                return $"{item.description}\n(Currently: +{ouroMult:0.##}x Base Mult, +{ouroScore} Base Score)";

            default:
                return item.description;
        }
    }
}