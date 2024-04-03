using UnityEngine;

public class FireTrailCard : PassiveCard
{
    [SerializeField] private float trailDuration;
    [SerializeField] private float fireDamage;

    public float TrailDuration { get => trailDuration; set => trailDuration = value; }
    public float FireDamage { get => fireDamage; set => fireDamage = value; }

    public override void ApplyRarityAdjustments(Rarity rarity)
    {
        // Implement rarity adjustments
    }
}