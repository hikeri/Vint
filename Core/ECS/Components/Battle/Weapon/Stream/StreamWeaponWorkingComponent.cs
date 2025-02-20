using Vint.Core.Battles.Weapons;
using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;
using Vint.Core.Server;

namespace Vint.Core.ECS.Components.Battle.Weapon.Stream;

[ProtocolId(971549724137995758)]
public class StreamWeaponWorkingComponent : IComponent { // todo modules
    public int Time { get; private set; }

    public void Removed(IPlayerConnection connection, IEntity entity) =>
        (connection.BattlePlayer?.Tank?.WeaponHandler as StreamWeaponHandler)?.IncarnationIdToHitTime.Clear();
}