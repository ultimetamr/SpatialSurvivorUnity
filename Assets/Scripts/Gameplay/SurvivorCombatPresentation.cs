using UnityEngine;

namespace SpatialSurvivor {
public sealed partial class SurvivorGame {
    // Combat presentation uses the same clock as damage and movement. Menus,
    // tracking loss and hit-stop freeze transient rings and hit reactions too.
    void CombatPresentationStep(float dt) {
        CombatFeelStep(dt);
        foreach (var effect in fx) {
            if (effect.life <= 0) continue;
            effect.life = Mathf.Max(0, effect.life - dt);
            if (effect.life <= 0) effect.line.enabled = false;
            else if (effect.ring)
                RingShape(effect.line, effect.center, effect.radius * (1 - effect.life / effect.max));
        }
        foreach (var enemy in enemies) {
            if(enemy.healthBar)enemy.healthBar.Tick(dt);
            if (!enemy.active || !enemy.visual) continue;
            enemy.visual.transform.localScale = Vector3.Lerp(enemy.visual.transform.localScale,
                enemy.baseScale, 1 - Mathf.Exp(-dt * 18));
        }
    }
}
}
