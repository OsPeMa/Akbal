# Akbal — Game Design Document

*Working design document. Last updated 2026-05-22. Tracks both committed design decisions and the implemented prototype.*

---

## 1. Vision

Akbal is an isometric rhythm-action game where the player does not defeat enemies by killing them — they heal and purify them. Enemies are corrupted or enraged; the player reduces their corruption through gameplay. Combat resolution is healing, not damage.

## 2. Design pillars

1. **You don't kill, you purify.** Every fight ends with an enemy restored, not destroyed. This is the thematic and mechanical north star — no system should contradict it.
2. **The world is the music.** Enemies, attacks, and ritual all live on a shared beat grid. The song never stops during an encounter.
3. **Two timings, one nervous system.** Action phase rewards reactive timing (dodge, parry). Ritual phase rewards predictive timing (rhythm). Both share the song's tempo so player skill carries seamlessly across.

## 3. Core gameplay loop

```
Encounter starts → song begins (Conductor.Play)
   ↓
Enemy in Attacking phase
   - Telegraphs, strikes on the beat
   - Player dodges (movement) or parries (timing input)
   - Successful parries fill the enemy's pressure meter
     (on-beat fills more, off-beat fills less, both block damage)
   ↓
Stun trigger fires when either condition is met:
   - Pressure meter reaches threshold (player-driven, faster)
   - Patterned vulnerable window arrives (scheduled, baseline fallback)
   ↓
Enemy in Stunned phase (vulnerable window opens)
   ↓
Player presses E within range → ritual opens
   ↓
Enemy in Ritualling phase, locked
   - Player taps arrow keys aligned to the song's beat
   - Each Perfect/Good press applies a chunk of purify (to enemy)
     AND a chunk of heal (to player) immediately
   - Corruption bar and HP bar visibly track each press
   ↓
Pattern completes one pass → Session.Finished → ritual auto-closes
   ↓
Enemy in Resolving phase (brief, non-targetable)
   ↓
Back to Attacking phase, OR enemy dies if corruption reduced to zero
```

The song is the encounter's structure. The encounter is the song made playable.

## 4. Audio architecture: continuous music

- Music plays without restart or switch for the entire encounter.
- The `Conductor` singleton (`Assets/Scripts/Rhythm/Conductor.cs`) owns the audio source and exposes song time in beats via `AudioSettings.dspTime`.
- If no audio clip is set, the Conductor procedurally generates a 120 BPM metronome on Awake for prototype-friendly debugging.
- Ritual mode is an **interaction toggle** on rhythm that is already audible — not a separate mode with its own track.
- **Outside ritual mode:** rhythm is ambient. No penalties for off-beat or missed inputs.
- **Inside ritual mode:** inputs become mandatory. Performance directly affects healing/purification output.
- Rhythm awareness is rewarded in the action phase (parry economy) but never required.

The continuous-music decision is load-bearing — it eliminates the mode-switch jolt that would otherwise sit between action and rhythm phases.

## 5. Action phase

### Verbs

- **Move.** Free-form isometric movement (`PlayerMover`, WASD or left stick). Positioning and spacing.
- **Dodge.** Pure survival (`PlayerDash`, Space or south face button). Binary outcome — avoided or hit. No rhythm reward and does not advance the encounter — it's strict survival.
- **Parry.** The skill expression verb (`PlayerParry`, Q or north face button). Timing input dressed as a combat action. Primary bridge between action and rhythm.

### Parry economy

The parry input opens a brief active window during which a strike will be blocked. At the moment of press, timing is classified against the conductor's current sub-beat.

| Parry timing | Effect on the incoming strike | Effect on the enemy's pressure meter |
|---|---|---|
| Off-beat (any timing within parry window) | Blocks damage | Adds `pressurePerOffBeatParry` (default 1.0) |
| On-beat (within ±`onBeatThreshold` of an integer beat) | Blocks damage | Adds `pressurePerOnBeatParry` (default 2.0) |

Off-beat parries never punish — the player just gets less. Rhythm awareness is mechanically meaningful but not gated.

### Why parry is prioritized over dodge

Parry is a timing input. It is a rhythm note dressed in action-game clothes. Pushing skill expression toward parry means players are practicing rhythm without realizing it — by the time ritual mode opens, their timing is already warm.

## 6. Ritual phase (healing)

### Trigger

The enemy enters the **Stunned** phase when either of two conditions is met (rising-edge guarded so only one trigger per pattern cycle):

1. **Pressure-triggered.** `Pressure ≥ Archetype.pressureThreshold` (default 3.0). Skilled, active route.
2. **Patterned vulnerable window.** The pattern playhead enters `[vulnerableStartBeat, vulnerableEndBeat)`. Passive, baseline route — guarantees novices a ritual opportunity each cycle.

Either source uses the same Stunned phase. Stunned lasts `Archetype.stunDurationBeats` (default 4 beats); if the player doesn't open a ritual in that window, the enemy returns to Attacking.

### Ritual activation

Player presses E while within `purifyRange` of a stunned enemy. The minigame opens with the enemy's archetype-specific `ritualPattern` (or falls back to the global default on the `RhythmMinigame` component if the archetype's is null). The enemy enters the **Ritualling** phase and is locked there until the rhythm pattern completes one pass.

### Performance and effect — per-hit application

Each arrow-key press during ritual is classified as Perfect, Good, or Miss against the nearest active beat. Successful presses apply effects **immediately**, not at the end:

| Press grade | Per-hit purify (enemy corruption ↓) | Per-hit heal (player HP ↑) |
|---|---|---|
| Perfect | `PerfectPurify` (default 2.0) | `PerfectHeal` (default 1.0) |
| Good | `GoodPurify` (default 1.0) | `GoodHeal` (default 0.5) |
| Miss | — | — |

The corruption bar visibly shrinks each correct press. The player's HP bar visibly fills. The bars are the feedback; there is no separate "results screen" at the end.

When `Session.Finished` fires (pattern played through one pass), the ritual auto-closes — no need to hold E.

## 7. Enemy archetypes

Input complexity scales with archetype, not with linear game progression. Archetypes are introduced in order but tied to encounter content, not a global unlock counter.

| Archetype | Buttons | Role | Teaches | Implementation |
|---|---|---|---|---|
| Drum | 1 | First encounters, intro fights | Timing fundamentals | **Implemented** as the default prototype archetype |
| Bass | 2 | Mid-tier enemies | Coordination, syncopation | Design only |
| Lead | 3 | Advanced enemies | Sequence and melody | Design only |
| Full band | 4 | Bosses | Integration of everything | Design only |

### Identity rules

- Each enemy archetype is an `EnemyArchetype` ScriptableObject bundling pattern, tuning, visuals.
- Attacks are authored as beat-event timelines (see § 9), not procedural patterns.
- Visual state should reflect corruption level (designed; not yet implemented): more corrupted = saturated, aggressive. More lucid = softer palette, slower animations.

## 8. Enemy lifecycle (Phase model)

Each enemy has a top-level **Phase** that drives its behavior. Phases are enumerated in `Enemy.Phase`:

```
       ┌─────────────┐
       │  Attacking  │←──────────────────────────────┐
       └──────┬──────┘                               │
              │ pressure ≥ threshold                 │
              │   OR patterned vuln. window opens    │
              ↓                                      │
       ┌─────────────┐                               │
       │   Stunned   │──────────────────────────────►│ stunDurationBeats elapsed
       └──────┬──────┘                               │
              │ player presses E in range            │
              ↓                                      │
       ┌─────────────┐                               │
       │ Ritualling  │                               │
       └──────┬──────┘                               │
              │ Session.Finished                     │
              ↓                                      │
       ┌─────────────┐                               │
       │  Resolving  │──────────────────────────────►┘ resolveDurationBeats elapsed
       └─────────────┘
```

Inside the **Attacking** phase, per-frame behavior is derived from the current `patternBeat`:

- `Chasing` — out of attack range; moves toward the player
- `Telegraphing` — within an attack event's pre-strike window; stops, faces the player, color = telegraph color
- `Striking` — within an attack event's strike window; lunges forward and runs `OverlapSphere` for player/parry detection

`Stunned`, `Ritualling`, and `Resolving` all freeze movement and apply the vulnerable color. The three are visually similar but mechanically distinct: only `Stunned` allows ritual opening, only `Ritualling` is locked against re-targeting, and `Resolving` is a brief breathing beat before re-entering combat.

`CanReceiveRitual` returns true only during `Stunned`. `IsVulnerable` (whether purify damage applies) returns true during `Stunned` and `Ritualling`.

## 9. Encounter structure

### Authoring layers (three ScriptableObjects)

1. **`EnemyAttackPattern`** (`Assets/Scripts/Enemy/EnemyAttackPattern.cs`)
   - `totalBeats` — cycle length (default 8)
   - `attacks` — list of `(beat, telegraphBeats, kind)` events
   - `vulnerableStartBeat` / `vulnerableEndBeat` — scheduled vulnerable window

2. **`EnemyArchetype`** (`Assets/Scripts/Enemy/EnemyArchetype.cs`) — the identity layer. Bundles:
   - `attackPattern` reference
   - `ritualPattern` reference (optional; falls back to RhythmMinigame's default)
   - `maxCorruption` (HP pool, default 5)
   - Pressure tuning: `pressureThreshold`, `pressurePerOnBeatParry`, `pressurePerOffBeatParry`
   - Phase durations: `stunDurationBeats`, `resolveDurationBeats`
   - Movement/strike: `moveSpeed`, `attackRange`, `strikeReach`, `strikeRadius`, `strikeDamage`, `strikeDurationBeats`
   - Visuals: `baseColor`, `telegraphColor`, `vulnerableColor`

3. **`Encounter`** (`Assets/Scripts/Encounter/Encounter.cs`) — the casting layer. List of `EnemySpawn { archetype, positionOffset, beatOffset }`. Defines who is in the fight, where, and how their pattern playheads are offset.

At runtime, `EncounterRunner` (`Assets/Scripts/Encounter/EncounterRunner.cs`) reads an `Encounter` and instantiates the enemy GameObjects.

### Multi-enemy choreography via beat offset

Multiple enemies share `Conductor.SongBeats` but each enemy reads its pattern playhead as `(songBeats - beatOffset) % pattern.totalBeats`. By assigning different `beatOffset` values per spawn, you stagger their cycles:

| Enemy | beatOffset | Strikes happen at song beats… |
|---|---|---|
| A | 0.00 | 1, 3 (then 9, 11, …) |
| B | 2.67 | 3.67, 5.67 (then 11.67, 13.67, …) |
| C | 5.33 | 6.33, 8.33 (then 14.33, 16.33, …) |

This produces call-and-response naturally — enemies share the beat grid but rarely attack on the same instant. Their vulnerable windows are also staggered, so the player typically has at least one ritual target available at any given moment.

Simultaneous attacks (the drop, the chorus hit) are still achievable by authoring overlapping offsets when designed.

### Attack kinds (extension point)

`AttackEvent.kind` is an enum:

- **Melee** — implemented; enemy lunges forward by `strikeReach` and damages via `OverlapSphere`
- **Ranged** — placeholder; intended to spawn a projectile on the strike beat
- **AreaOfEffect** — placeholder; intended to damage all targets inside a circular indicator on the strike beat

Today only Melee deals damage; Ranged/AreaOfEffect events short-circuit harmlessly so they can sit in patterns as content placeholders without breaking.

## 10. Audio and visual integration

- Background music drives `Conductor`, which everything else reads from.
- Ritual mode entry does not restart the song; the rhythm phrase aligns to the song's existing beat grid.
- **Player HP bar** (top-left, `HealthBarHUD`): standard bar, fills during ritual heals.
- **Per-enemy corruption bar** (`EnemyCorruptionBar`, world-tracked above each enemy via `Camera.WorldToScreenPoint`): fill width = `Health.Fraction`; color lerps from corruption-red toward lucid-cyan as the enemy is purified.
- **Conductor debug HUD** (top-center, `ConductorDebugHUD`): BPM, current beat, current bar; dot pulses on every beat, tinted differently on downbeats.
- **Rhythm feedback HUD** (centered, just below the wheel, `RhythmFeedbackHUD`): "Perfect!" / "Good!" / "Miss!" text pops on each press, color-coded; fades over half a second.
- Player capsule **flashes color** briefly on each parry — cyan for off-beat, gold for on-beat.

Planned but not implemented: audio ducking on ritual entry, beat-aligned visual transitions into ritual, world-space pulse on the ground.

## 11. Rejected design directions

Documented so they are not relitigated without new information.

- **Active combat-ritual buffs** (speed / armor / spells). Violates the "reduce cognitive overload" goal, dilutes the purification premise, steals the climax from the vulnerability moment, softens the action phase's stakes.
- **Self-heal button at stun.** Lets players skip the core rhythm mechanic, makes vulnerability feel like a vending machine, contradicts the worldbuilding (healing comes from ritual, not button press).
- **Procedural min/max parry-dodge counter.** Pacing should be controlled by the song author through authored attack timelines, not a runtime system.
- **Linear input unlock (1 → 2 → 3 → 4 by game progression).** Brittle against players with different skill curves. Replaced with archetype-driven input complexity.
- **Delayed (end-of-ritual) corruption resolution.** Rejected during implementation: per-hit application gives essential moment-to-moment feedback ("I felt that press matter"). The corruption bar shrinking live IS the feedback.
- **Ritual charges as a separate spendable currency.** On-beat parry already advances the encounter via the pressure meter; adding a separate charge layer added no new decision-making for the player.
- **Pure parry-triggered stun (removing the scheduled window).** Would punish novices who can't parry well — they'd never get a ritual opportunity. Kept the patterned vulnerable window as a baseline alongside pressure.
- **Dodge-triggers-vulnerability path** (previously in code as "missed strike → vulnerable"). Removed when patterned vulnerability landed — dodge is now strictly survival; only patterned schedule and pressure open ritual windows.

## 12. Open questions

These should be resolved via prototype, not on paper.

| Question | Status |
|---|---|
| Does the world pause during ritual mode? | **Resolved.** No — world continues. Player moves at 0.4× during ritual; other enemies still attack. |
| One song per enemy archetype vs per encounter? | **Open.** Per-archetype ritual pattern is supported. Encounter-level song coordination not implemented. |
| Visual representation of beat | **Partial.** Conductor debug HUD pulse exists. World-space pulse on the ground not implemented. |
| Does on-beat parry feel different to playtesters without being told? | **Unresolved.** Needs playtest. |
| Should multiple enemies share a song or have individual songs? | **Open.** Currently all share via global `Conductor` + per-enemy `beatOffset` stagger. Individual songs is a possible future extension (would require a per-enemy audio source layer). |
| When (in the song) does a fully purified enemy get celebrated? Audio sting on next downbeat? Visual dissolve aligned to bar boundary? | **Open.** Currently no special audio/visual on purify completion. |
| If `RhythmPattern.totalBeats` equals `EnemyAttackPattern.totalBeats`, can the player chain rituals? | **Mitigated.** Default ritual is 4 beats and enemy pattern is 8 beats. If they ever match, the post-ritual `Resolving` phase (default 1 beat) is the firewall against immediate re-ritualling. May need a longer firewall once tunable lengths land. |

## 13. Glossary

- **Action phase** — Free-form combat: movement, dodge, parry. Music ambient. Corresponds to `Enemy.Phase.Attacking`.
- **Ritual phase / Ritual mode** — Healing interaction. Inputs mandatory, performance scales output per-press. Corresponds to `Enemy.Phase.Ritualling`.
- **Conductor** — `Assets/Scripts/Rhythm/Conductor.cs`. Singleton holding song time in beats, BPM, beat/bar events. Generates a metronome AudioClip procedurally if no song is set.
- **Pressure** — Per-enemy meter that fills with parries (more from on-beat, less from off-beat). When it reaches `Archetype.pressureThreshold`, the enemy enters Stunned. Resets on Stun entry.
- **Vulnerable window** — A scheduled portion of the `EnemyAttackPattern` cycle (`vulnerableStartBeat` to `vulnerableEndBeat`) during which the enemy automatically becomes Stunned. Rising-edge triggered, once per cycle.
- **Stunned** — Enemy phase in which ritual mode can be activated. Lasts `stunDurationBeats` if no ritual is started.
- **Ritualling** — Enemy phase during an active ritual. The enemy is locked here until `RhythmSession.Finished`. Still IsVulnerable for per-hit purify.
- **Resolving** — Brief post-ritual phase (default 1 beat) before the enemy returns to Attacking. Buffer against immediate re-ritualling and a moment for visual/audio celebration of the purification.
- **Beat offset** — Per-enemy phase offset (in beats) applied to its attack pattern playhead. Used to stagger multi-enemy choreography.
- **Encounter** — A single fight: one shared song clock, one or more enemies. Defined by an `Encounter` SO containing `EnemySpawn` entries.
- **Archetype** — An enemy identity, defined by an `EnemyArchetype` SO. Bundles attack pattern, ritual pattern, corruption pool, tuning, and visuals. Drum / Bass / Lead / Boss are the planned archetypes.
- **Attack kind** — Enum on `AttackEvent` (Melee / Ranged / AreaOfEffect). Today only Melee deals damage; others are placeholders.
