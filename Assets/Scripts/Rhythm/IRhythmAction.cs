public interface IRhythmAction
{
    bool IsValid();
    void Apply(RhythmSession session);
}

public class PurifyAction : IRhythmAction
{
    public Enemy Target { get; }
    public Health PlayerHealth { get; }
    public float PerfectPurify { get; set; } = 2.0f;
    public float GoodPurify { get; set; } = 1.0f;
    public float PerfectHeal { get; set; } = 1.0f;
    public float GoodHeal { get; set; } = 0.5f;

    public PurifyAction(Enemy target, Health playerHealth)
    {
        Target = target;
        PlayerHealth = playerHealth;
    }

    public bool IsValid() => Target != null && Target.IsVulnerable;

    public void Apply(RhythmSession session)
    {
        if (Target == null || session == null) return;
        float purifyAmt = session.PerfectCount * PerfectPurify + session.GoodCount * GoodPurify;
        float healAmt   = session.PerfectCount * PerfectHeal   + session.GoodCount * GoodHeal;
        if (purifyAmt > 0f) Target.Purify(purifyAmt);
        if (healAmt > 0f && PlayerHealth != null) PlayerHealth.Heal(healAmt);
    }
}
