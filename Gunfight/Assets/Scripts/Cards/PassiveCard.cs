using UnityEngine;

public abstract class PassiveCard : MonoBehaviour, ICard
{
    [SerializeField] private string cardName;
    [SerializeField] private Rarity rarity;

    public string CardName { get => cardName; }
    public Rarity Rarity { get => rarity; set => rarity = value; }

    // Base method for applying rarity adjustments
    public abstract void ApplyRarityAdjustments(Rarity rarity);
}