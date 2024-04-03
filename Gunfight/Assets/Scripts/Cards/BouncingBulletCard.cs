using UnityEngine;

public class BouncingBulletCard : PassiveCard
{
    [SerializeField] private int bouncePerShot;

    public int BouncePerShot { get => bouncePerShot; set => bouncePerShot = value; }

    public override void ApplyRarityAdjustments(Rarity rarity)
    {
        // Implement rarity adjustments for Bouncing Bullet
    }
}