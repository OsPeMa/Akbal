using System.Collections.Generic;
using UnityEngine;

public static class EnemyRegistry
{
    static readonly List<Enemy> all = new List<Enemy>();
    public static IReadOnlyList<Enemy> All => all;

    public static void Register(Enemy e)
    {
        if (e != null && !all.Contains(e)) all.Add(e);
    }

    public static void Unregister(Enemy e)
    {
        all.Remove(e);
    }

    public static Enemy FindNearestVulnerable(Vector3 origin, float maxRange)
    {
        Enemy best = null;
        float bestDist = maxRange;
        for (int i = 0; i < all.Count; i++)
        {
            var e = all[i];
            if (e == null || !e.IsVulnerable) continue;
            float d = Vector3.Distance(origin, e.transform.position);
            if (d <= bestDist) { bestDist = d; best = e; }
        }
        return best;
    }
}
