# Akbal — Game Design Document

*Working design document. Last updated 2026-05-22.*

---

## 1. Vision

Akbal is an isometric rhythm-action game where the player does not defeat enemies by killing them — they heal and purify them. Enemies are corrupted or enraged; the player reduces their corruption through gameplay. Combat resolution is healing, not damage.

## 2. Design pillars

1. **You don't kill, you purify.** Every fight ends with an enemy restored, not destroyed. This is the thematic and mechanical north star — no system should contradict it.
2. **The world is the music.** Enemies, attacks, and ritual all live on a shared beat grid. The song never stops during an encounter.
3. **Two timings, one nervous system.** Action phase rewards reactive timing (dodge, parry). Ritual phase rewards predictive timing (rhythm). Both share the song's tempo so player skill carries seamlessly across.

## 3. Core gameplay loop

```
Encounter starts → song begins
   ↓
Action phase (continuous music, rhythm ambient)
   - Player moves, dodges (free-form survival)
   - Player parries on enemy telegraphs
   - On-beat parries banked as ritual reward
   ↓
Enemy reaches exhaustion threshold
   ↓
Vulnerability lands on next song-structure boundary (verse end / chorus)
   ↓
Ritual phase (same song continues, inputs become mandatory)
   - Player performs rhythm phrase
   - Performance scales healing output
   - Enemy purified + player healed (coupled)
   ↓
Song resolves OR transitions to next encounter section
```

The song is the encounter's structure. The encounter is the song made playable.

## 4. Core system: continuous music

- Music plays without restart or switch for the entire encounter.
- Ritual mode is an **interaction toggle** on rhythm that is already audible — not a separate mode with its own track.
- **Outside ritual mode:** rhythm is ambient. No penalties for off-beat or missed inputs.
- **Inside ritual mode:** inputs become mandatory. Performance directly affects healing/purification output.
- Rhythm awareness is rewarded throughout (see Parry Economy) but never required outside ritual.

The continuous-music decision is load-bearing — it eliminates the mode-switch jolt that would otherwise sit between action and rhythm phases.

## 5. Action phase

### Verbs

- **Move.** Free-form isometric movement. Positioning and spacing.
- **Dodge.** Pure survival. Binary outcome — avoided or hit. No rhythm reward.
- **Parry.** The skill expression verb. Timing input dressed as a combat action. Primary bridge between action and rhythm.

### Parry economy

| Parry timing | Outcome |
|---|---|
| Off-beat (within parry window) | Blocks damage. Small contribution to enemy exhaustion. |
| On-beat (within parry window AND aligned to song beat) | Blocks damage. Larger exhaustion contribution. Banks a ritual charge. |

Off-beat parries never punish — the player just gets less. This preserves the "no penalty for missing the beat" rule while making rhythm awareness mechanically meaningful.

### Why parry is prioritized over dodge

Parry is a timing input. It is a rhythm note dressed in action-game clothes. Pushing skill expression toward parry means players are practicing rhythm without realizing it — by the time ritual mode opens, their timing is already warm.

## 6. Ritual phase (healing)

### Trigger

- Enemy exhaustion reaches threshold → enemy enters a *waiting* state.
- On the next song-structure boundary (verse end, chorus start, bridge), enemy enters vulnerability.
- Ritual mode toggles on. Music continues uninterrupted.

### Performance

- Player executes a rhythm phrase aligned to the current song section.
- Phrase length is set by the song section (verse remainder, full chorus, etc.).
- Performance is graded continuously — Perfect, Good, Off.

### Coupled healing

The healing ritual heals the enemy AND the player. Both scale with rhythm performance.

| Performance | Enemy purification | Player healed |
|---|---|---|
| Perfect | Fully purified | Fully healed |
| Sloppy | Partially purified | Partially healed |
| Failed | Minimal purification | Minimal healing |

**Why coupled:** preserves the thematic premise (purification restores both parties), keeps the rhythm mechanic non-skippable, and prevents the "vulnerability moment as vending machine" problem that a separate self-heal button would create.

### Ritual charges

Banked from on-beat parries during action phase. Spent automatically during ritual mode as performance multipliers or safety nets. The decision-making lives in the action phase — players don't manage a menu during ritual.

## 7. Enemy design

### Archetypes (drives input progression)

Input complexity scales with archetype, not with linear game progression. Archetypes are introduced in order but tied to encounter content, not a global unlock counter.

| Archetype | Buttons | Role | Teaches |
|---|---|---|---|
| Drum | 1 | First encounters, intro fights | Timing fundamentals |
| Bass | 2 | Mid-tier enemies | Coordination, syncopation |
| Lead | 3 | Advanced enemies | Sequence and melody |
| Full band | 4 | Bosses | Integration of everything |

### Identity

- Each enemy is tied to a song or song role within an encounter.
- Attacks are authored as beat-event timelines, not procedural patterns.
- Visual state reflects corruption level: more corrupted = saturated, jagged, aggressive. More lucid = softer palette, slower animations, gentler attacks.

## 8. Encounter structure

- Each encounter is a single song, 1–2 enemies.
- The song's structure (intro / verse / chorus / bridge / outro) defines the encounter's pacing.
- Vulnerability windows are designed to land on song boundaries. Composer and combat designer are the same role.
- Attack patterns are authored as `(beat offset, attack type, parry-or-dodge)` events on a timeline. Pacing is a composer concern, not a runtime system.

### Multi-enemy choreography

When two enemies are present:

- They share the beat grid but rarely attack on the same beat.
- Default pattern: call-and-response. One leads, one answers.
- Simultaneous attacks are reserved for designed musical moments (the drop, the chorus hit). When they occur, the parry reward is exceptional.

## 9. Audio and visual integration

- Background music drives the conductor that everything else reads from.
- Ritual mode entry: audio ducking emphasizes the lead voice; subtle visual transition aligned to the next beat. Not a screen wipe — a focus shift.
- Both bars (enemy corruption, player HP) visible during ritual phase, pulsing on-beat, sold together as "purification restores both of you."

## 10. Rejected design directions

Documented so they are not relitigated without new information.

- **Active combat-ritual buffs (speed / armor / spells).** Rejected: violates the "reduce cognitive overload" goal, dilutes the purification premise, steals the climax from the vulnerability moment, softens the action phase's stakes.
- **Self-heal button at stun.** Rejected: lets players skip the core rhythm mechanic, makes vulnerability feel like a vending machine, contradicts the worldbuilding (healing comes from ritual, not button press).
- **Procedural min/max parry-dodge counter.** Rejected: pacing should be controlled by the song author through authored attack timelines, not a runtime system.
- **Linear input unlock (1 → 2 → 3 → 4 by game progression).** Rejected: brittle against players with different skill curves. Replaced with archetype-driven input complexity.

## 11. Open questions

These should be resolved via prototype, not on paper.

1. Does the world pause during ritual mode, or does the song continue with other enemies still attacking?
2. One song per enemy archetype (cheap, reusable) or per encounter (memorable, expensive content cost)?
3. Visual representation of beat: world-space pulse, UI metronome, both, neither?
4. Does on-beat parry feel different from off-beat parry to playtesters without being told?

## 12. Glossary

- **Action phase** — Free-form combat: movement, dodge, parry. Music ambient.
- **Ritual phase / Ritual mode** — Healing interaction. Inputs mandatory, performance scales output.
- **Conductor** — Internal system holding song time, beat index, and section events. Single source of truth.
- **Exhaustion** — The accumulated state from successful parries that eventually drives an enemy into vulnerability.
- **Vulnerability** — Enemy state in which ritual mode can be activated. Lands on song-structure boundaries.
- **Ritual charge** — Currency banked by on-beat parries, spent during ritual mode as performance multiplier.
- **Encounter** — A single fight: one song, one or two enemies, one arena.
