using Vint.Core.Battles.Player;
using Vint.Core.ChatCommands.Attributes;
using Vint.Core.Database.Models;

namespace Vint.Core.ChatCommands.Modules;

[ChatCommandGroup("tester", "Commands for testers", PlayerGroups.None)] // todo PlayerGroups.None yet
public class TesterModule : ChatCommandModule {
    [RequireConditions(ChatCommandConditions.InBattle), ChatCommand("spawnpoint", "Get last spawn point coordinates")]
    public void SpawnPoint(ChatCommandContext ctx) {
        BattleTank battleTank = ctx.Connection.BattlePlayer!.Tank!;

        ctx.SendPrivateResponse($"Previous: {battleTank.PreviousSpawnPoint}");
        ctx.SendPrivateResponse($"Current: {battleTank.SpawnPoint}");
    }
}