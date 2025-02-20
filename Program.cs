﻿using System.Net;
using Serilog;
using Serilog.Events;
using Vint.Core.Config;
using Vint.Core.Database;
using Vint.Core.Server;
using Vint.Core.Utils;

namespace Vint;

abstract class Program {
    static ILogger Logger { get; set; } = null!;

    static Task Main() {
        LoggerUtils.Initialize(LogEventLevel.Information);

        Logger = Log.Logger.ForType(typeof(Program));

        DatabaseConfig.Initialize();

        StaticServer staticServer = new(IPAddress.Any, 8080);
        GameServer gameServer = new(IPAddress.Any, 5050);

        ConfigManager.InitializeCache();
        ConfigManager.InitializeNodes();
        ConfigManager.InitializeGlobalEntities();
        ConfigManager.InitializeMapInfos();

        new Thread(() => staticServer.Start()) { Name = "Static Server" }.Start();
        new Thread(() => gameServer.Start()) { Name = "Game Server" }.Start();

        return Task.Delay(-1);
    }
}