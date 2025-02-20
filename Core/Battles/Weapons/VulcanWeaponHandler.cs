using Vint.Core.Battles.Damage;
using Vint.Core.Battles.Player;
using Vint.Core.Battles.States;
using Vint.Core.Config;
using Vint.Core.ECS.Components.Server;
using Vint.Core.ECS.Events.Battle.Weapon.Hit;

namespace Vint.Core.Battles.Weapons;

public class VulcanWeaponHandler : StreamWeaponHandler, IHeatWeaponHandler {
    public VulcanWeaponHandler(BattleTank battleTank) : base(battleTank) {
        OverheatingTime = TimeSpan.FromSeconds(ConfigManager.GetComponent<TemperatureHittingTimePropertyComponent>(MarketConfigPath).FinalValue);
        HeatDamage = ConfigManager.GetComponent<HeatDamagePropertyComponent>(MarketConfigPath).FinalValue;
        TemperatureLimit = ConfigManager.GetComponent<TemperatureLimitPropertyComponent>(MarketConfigPath).FinalValue;
        TemperatureDelta =
            ConfigManager.GetComponent<DeltaTemperaturePerSecondPropertyComponent>(MarketConfigPath).FinalValue * (float)Cooldown.TotalSeconds;
    }

    public override int MaxHitTargets => 1;

    public DateTimeOffset? ShootingStartTime { get; set; }
    public DateTimeOffset? LastOverheatingUpdate { get; set; }
    TimeSpan OverheatingTime { get; }

    bool IsOverheating => ShootingStartTime.HasValue &&
                          DateTimeOffset.UtcNow - ShootingStartTime >= OverheatingTime;
    public override float TemperatureLimit { get; }
    public override float TemperatureDelta { get; }
    public float HeatDamage { get; }

    public override void Fire(HitTarget target) {
        long incarnationId = target.IncarnationEntity.Id;

        if (IsCooldownActive(incarnationId)) return;

        Battle battle = BattleTank.Battle;
        BattleTank targetTank = battle.Players
            .Where(battlePlayer => battlePlayer.InBattleAsTank)
            .Select(battlePlayer => battlePlayer.Tank!)
            .Single(battleTank => battleTank.Incarnation == target.IncarnationEntity);

        bool isEnemy = BattleTank.IsEnemy(targetTank);

        // ReSharper disable once ArrangeRedundantParentheses
        if (targetTank.StateManager.CurrentState is not Active ||
            (!isEnemy && !battle.Properties.FriendlyFire)) return;

        if (IsOverheating)
            targetTank.UpdateTemperatureAssists(BattleTank, false);

        CalculatedDamage damage = DamageCalculator.Calculate(BattleTank, targetTank, target);
        battle.DamageProcessor.Damage(BattleTank, targetTank, MarketEntity, damage);
    }

    public override void Tick() {
        base.Tick();
        UpdateOverheating();
    }

    public void UpdateOverheating() {
        if (!IsOverheating ||
            LastOverheatingUpdate.HasValue &&
            DateTimeOffset.UtcNow - LastOverheatingUpdate < Cooldown) return;

        if (BattleTank.StateManager.CurrentState is Dead) {
            ShootingStartTime = null;
            LastOverheatingUpdate = null;
            return;
        }

        BattleTank.UpdateTemperatureAssists(BattleTank, false);
        LastOverheatingUpdate = DateTimeOffset.UtcNow;
    }

    public override void OnTankDisable() {
        base.OnTankDisable();

        ShootingStartTime = null;
        LastOverheatingUpdate = null;
    }
}