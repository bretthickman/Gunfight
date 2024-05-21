// Interface for all cards (both active and passive)
public interface ICard
{
    string CardName { get; }
    Rarity Rarity { get; }
}

// Enum for rarity levels
public enum Rarity
{
    Common,
    Rare,
    Legendary
}
