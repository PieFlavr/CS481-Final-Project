/// <summary>
/// Numeric largely unchanging combat stats (base damage, speed, etc...)
/// </summary>
public class CombatStats
{
    public float Speed { get; private set; }
    public float BaseDamage { get; private set; }
    public float AttackRange { get; private set; }
    public float AttackRadius { get; private set; }

    public CombatStats() { }

    public CombatStats(float speed, float baseDamage, float attackRange = 1f, float attackRadius = 0.5f)
    {
        Speed = speed;
        BaseDamage = baseDamage;
        AttackRange = attackRange;
        AttackRadius = attackRadius;
    }

    public void Initialize(float speed, float baseDamage, float attackRange = 1f, float attackRadius = 0.5f)
    {
        Speed = speed;
        BaseDamage = baseDamage;
        AttackRange = attackRange;
        AttackRadius = attackRadius;
    }

    public void SetSpeed(float newSpeed)
    {
        Speed = newSpeed;
    }

    public void SetDamage(float newDamage)
    {
        BaseDamage = newDamage;
    }

    public void SetAttackRange(float newRange)
    {
        AttackRange = newRange;
    }

    public void SetAttackRadius(float newRadius)
    {
        AttackRadius = newRadius;
    }
}
