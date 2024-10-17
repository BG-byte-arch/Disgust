using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "New Player Unit", menuName = "Player Unit")]
public class UnitClass : ScriptableObject
{
    [Header("Default Unit Settings")]
    public new string name;
    public Sprite spriteTexture;
    public int foodCost;
    public int attackDamage;
    public int hitpoint;
    public float hitboxSize;
    public float speed;
    public float attackDelay;
    [Header("Ranged Unit Settings")]
    public bool ranged;
    public float rangedAttackRange;
    public float rangedAttackDelay;
    public string rangedProjectileSpriteName;
    public Sprite rangedProjectileSpriteTexture;
    public int rangedProjectileDamage;
    public float rangedProjectileSpeed;
}
