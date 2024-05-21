using UnityEngine;

public class GrapplingGunCard : ActiveCard
{
    [SerializeField] private float distance;
    [SerializeField] private float cooldown;
    [SerializeField] private bool canGrapplePlayers;
    [SerializeField] private bool canHookPlayers;

    public float Distance { get => distance; set => distance = value; }
    public float Cooldown { get => cooldown; set => cooldown = value; }
    public bool CanGrapplePlayers { get => canGrapplePlayers; set => canGrapplePlayers = value; }
    public bool CanHookPlayers { get => canHookPlayers; set => canHookPlayers = value; }

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