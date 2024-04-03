using UnityEngine;

public abstract class ActiveCard : MonoBehaviour, ICard
{
    [SerializeField] private string cardName;
    [SerializeField] private Rarity rarity;

    public string CardName { get => cardName; }
    public Rarity Rarity { get => rarity; set => rarity = value; }

    // Base method for activating the card
    public abstract void ActivateCard();

    // Base method for applying rarity adjustments
    public abstract void ApplyRarityAdjustments(Rarity rarity);

    // Base method for applying modifiers
    public abstract void ApplyModifiers(string modifier);
}