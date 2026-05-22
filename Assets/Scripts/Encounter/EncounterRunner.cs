using UnityEngine;

public class EncounterRunner : MonoBehaviour
{
    public Encounter encounter;
    public Transform encounterOrigin;
    public string targetTag = "Player";
    public float groundY = 1f;

    void Start()
    {
        if (encounter == null) return;
        if (encounterOrigin == null)
        {
            var go = GameObject.FindGameObjectWithTag(targetTag);
            if (go != null) encounterOrigin = go.transform;
        }
        foreach (var spawn in encounter.spawns) SpawnOne(spawn);
    }

    void SpawnOne(EnemySpawn spawn)
    {
        if (spawn.archetype == null) return;

        Vector3 origin = encounterOrigin != null ? encounterOrigin.position : Vector3.zero;
        Vector3 pos = new Vector3(origin.x + spawn.positionOffset.x, groundY, origin.z + spawn.positionOffset.y);

        var go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        go.name = $"Enemy_{spawn.archetype.archetypeName}";
        go.transform.position = pos;
        var mr = go.GetComponent<MeshRenderer>();
        mr.sharedMaterial = new Material(mr.sharedMaterial) { color = spawn.archetype.baseColor };

        go.AddComponent<Rigidbody>();
        var health = go.AddComponent<Health>();
        health.SetMax(spawn.archetype.maxCorruption);

        var enemy = go.AddComponent<Enemy>();
        enemy.archetype = spawn.archetype;
        enemy.beatOffset = spawn.beatOffset;
        enemy.target = encounterOrigin;

        var bar = go.AddComponent<EnemyCorruptionBar>();
        bar.source = health;
    }
}
