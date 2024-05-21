using UnityEngine;

public class SetTrapsCard : ActiveCard
{
    [SerializeField] private int numberOfTraps;
    [SerializeField] private float detectionRadius;
    [SerializeField] private float explosionRadius;
    [SerializeField] private bool canSlowCharacter;
    [SerializeField] private bool canApplyPoison;

    public int NumberOfTraps { get => numberOfTraps; set => numberOfTraps = value; }
    public float DetectionRadius { get => detectionRadius; set => detectionRadius = value; }
    public float ExplosionRadius { get => explosionRadius; set => explosionRadius = value; }
    public bool CanSlowCharacter { get => canSlowCharacter; set => canSlowCharacter = value; }
    public bool CanApplyPoison { get => canApplyPoison; set => canApplyPoison = value; }

    public override void ActivateCard()
    {
        // Implement Set Traps activation logic
    }

    public override void ApplyRarityAdjustments(Rarity rarity)
    {
        // Implement rarity adjustments for Set Traps
    }

    public override void ApplyModifiers(string modifier)
    {
        // Implement modifiers for Set Traps
    }
}