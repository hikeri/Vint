using Vint.Core.Battles.Weapons;
using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;
using Vint.Core.Server;

namespace Vint.Core.ECS.Events.Battle.Weapon.Shot.Hammer;

[ProtocolId(-1937089974629265090)]
public class SelfHammerShotEvent : SelfShotEvent {
    public int RandomSeed { get; private set; }

    protected override RemoteHammerShotEvent RemoteEvent => new() {
        RandomSeed = RandomSeed,
        ShotDirection = ShotDirection,
        ShotId = ShotId,
        ClientTime = ClientTime
    };

    public override void Execute(IPlayerConnection connection, IEnumerable<IEntity> entities) {
        if (!connection.InLobby ||
            !connection.BattlePlayer!.InBattleAsTank ||
            connection.BattlePlayer.Tank!.WeaponHandler is not HammerWeaponHandler weaponHandler) return;

        weaponHandler.SetCurrentCartridgeCount(weaponHandler.CurrentCartridgeCount - 1);
        base.Execute(connection, entities);
    }
}