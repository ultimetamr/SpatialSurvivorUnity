using UnityEngine;

namespace SpatialSurvivor {
public sealed partial class SurvivorGame {
    sealed class Crystal {
        public GameObject go;
        public bool active, attracting;
        public int exp;
        public float speed, animation;
        public Vector3 position;
    }

    void Drop(Vector3 position, int baseExperience) {
        if (screen != ScreenState.Playing || baseExperience <= 0) return;
        // Resolve each drop before merging so rounding stays identical when
        // the bounded pool fills, including fractional permanent EXP bonuses.
        int experience = Mathf.CeilToInt(baseExperience * player.experienceMultiplier);
        foreach (var crystal in crystals) {
            if (crystal.active) continue;
            crystal.active = true;
            crystal.attracting = false;
            crystal.speed = crystal.animation = 0;
            crystal.exp = experience;
            crystal.position = position + Vector3.up * .12f;
            crystal.go.transform.SetPositionAndRotation(crystal.position, Quaternion.Euler(45, 0, 45));
            crystal.go.SetActive(true);
            return;
        }
        Crystal nearest = null;
        float distance = float.MaxValue;
        foreach (var crystal in crystals) {
            float squared = (crystal.position - position).sqrMagnitude;
            if (squared >= distance) continue;
            distance = squared;
            nearest = crystal;
        }
        if (nearest != null) nearest.exp += experience;
    }

    void CrystalStep(float dt) {
        var target = PlayerPosition + Vector3.up * .12f;
        float pickup = player.pickup * (player.Owns(UpgradeId.MAGNETIC_FIELD) ? 2f :
                                      player.Owns(UpgradeId.EXPERIENCE_MAGNET) ? 1.35f : 1f);
        foreach (var crystal in crystals) {
            if (!crystal.active) continue;
            crystal.animation += dt;
            if (!crystal.attracting && Distance(crystal.position, target) <= pickup)
                crystal.attracting = true;
            // Once acquired, a crystal keeps chasing the player even if they
            // walk out of the original radius before it reaches them.
            if (crystal.attracting) {
                crystal.speed = Mathf.Min(6, crystal.speed + 7 * dt);
                crystal.position = Vector3.MoveTowards(crystal.position, target, crystal.speed * dt);
            }
            float hover = crystal.attracting ? 0 : Mathf.Sin(crystal.animation * Mathf.PI * 1.4f) * .045f;
            crystal.go.transform.position = crystal.position + Vector3.up * hover;
            crystal.go.transform.Rotate(0, dt * (crystal.attracting ? 220 : 75), 0, Space.World);
            if (!crystal.attracting || Vector3.Distance(crystal.position, target) > .3f) continue;
            crystal.active = false;
            crystal.go.SetActive(false);
            Cue(FeedbackCue.Pickup, crystal.position);
            AwardExperience(crystal.exp);
            if (screen != ScreenState.Playing) return;
        }
    }

    void AwardExperience(int experience) {
        if (screen != ScreenState.Playing || experience <= 0) return;
        player.experience += experience;
        if (player.experience < player.required) return;
        player.experience = 0;
        player.required = SurvivorRules.RequiredExp(player.required);
        player.level++;
        Cue(FeedbackCue.LevelUp, PlayerPosition);
        Ring(PlayerPosition, 2, Color.yellow, .6f);
        options = SurvivorRules.Draw(player, rng);
        if (options.Count == 0) return;
        reroll = player.Owns(UpgradeId.REROLL_MASTER);
        screen = ScreenState.Upgrade;
    }
}
}
