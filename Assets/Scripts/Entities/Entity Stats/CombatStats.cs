/// <summary>
/// Numeric largely unchanging combat stats (base damage, speed, etc...)
/// </summary>
public class CombatStats
{
    public float Speed { get; private set; }
    public float BaseDamage { get; private set; }

    public CombatStats() { }

    public CombatStats(float speed, float baseDamage)
    {
        Speed = speed;
        BaseDamage = baseDamage;
    }

    public void Initialize(float speed, float baseDamage)
    {
        Speed = speed;
        BaseDamage = baseDamage;
    }

    public void SetSpeed(float newSpeed)
    {
        Speed = newSpeed;
    }

    public void SetDamage(float newDamage)
    {
        BaseDamage = newDamage;
    }
}
