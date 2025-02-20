﻿using Serilog;
using Vint.Core.Database;
using Vint.Core.Database.Models;
using Vint.Core.ECS.Components.Entrance;
using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;
using Vint.Core.Server;
using Vint.Core.Utils;

namespace Vint.Core.ECS.Events.Entrance.RestorePassword;

[ProtocolId(1460106433434)]
public class RestorePasswordByEmailEvent : IServerEvent {
    public string Email { get; private set; } = null!;

    public void Execute(IPlayerConnection connection, IEnumerable<IEntity> entities) {
        ILogger logger = connection.Logger.ForType(GetType());

        logger.Warning("Restoring password '{Email}'", Email);

        using DbConnection db = new();
        Player player = db.Players.Single(player => player.Email == Email);

        connection.Player = player;

        //todo

        connection.ClientSession.AddComponent(new RestorePasswordCodeSentComponent(Email));
    }
}