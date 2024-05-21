using UnityEngine;

public class DodgeRollCard : ActiveCard
{
    [SerializeField] private float distance;
    [SerializeField] private float cooldown;
    [SerializeField] private bool canDropGrenade;
    [SerializeField] private bool canInvisible;

    public float Distance { get => distance; set => distance = value; }
    public float Cooldown { get => cooldown; set => cooldown = value; }
    public bool CanDropGrenade { get => canDropGrenade; set => canDropGrenade = value; }
    public bool CanInvisible { get => canInvisible; set => canInvisible = value; }

    public override void ActivateCard()
    {
        // Implement activation logic
    }

    public override void ApplyRarityAdjustments(Rarity rarity)
    {
        // Implement rarity adjustments
    }

    public override void ApplyModifiers(string modifier)
    {
        // Implement modifiers
    }
}