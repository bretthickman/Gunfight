using UnityEngine;

public class GrenadeCard : ActiveCard
{
    [SerializeField] private float damage;
    [SerializeField] private float throwableRadius;
    [SerializeField] private bool canPull;
    [SerializeField] private int canBounces;

    public float Damage { get => damage; set => damage = value; }
    public float ThrowableRadius { get => throwableRadius; set => throwableRadius = value; }
    public bool CanPull { get => canPull; set => canPull = value; }
    public int CanBounces { get => canBounces; set => canBounces = value; }

    public override void ActivateCard()
    {
        // Implement Grenade activation logic
    }

    public override void ApplyRarityAdjustments(Rarity rarity)
    {
        // Implement rarity adjustments for Grenade
    }

    public override void ApplyModifiers(string modifier)
    {
        // Implement modifiers for Grenade
    }
}