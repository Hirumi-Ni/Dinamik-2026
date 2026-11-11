using UnityEngine;

[CreateAssetMenu(fileName = "NewCardData", menuName = "Game Data/Card Data")]
public class CardDataSO : ScriptableObject
{
    [Header("Card Identity")]
    public string cardName;
    public Sprite cardIconSprite;
    public CardType cardType;

    [Header("Card Value")]
    public int cardBaseScore = 100;
    public bool isBomb = false;
}
