﻿using Discord;
using Discord.Commands;
using Discord.WebSocket;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using ScrubBot.Database;
using ScrubBot.Extensions;
using ScrubBot.Properties;

using System;
using System.Reflection;
using System.Threading.Tasks;

namespace ScrubBot.Handlers
{
    public class CommandHandler
    {
        private readonly DiscordSocketClient _client;
        private readonly CommandService _commandService;
        private readonly IServiceProvider _serviceProvider;

        private PrefixHandler _prefixHandler;

        public CommandHandler(ref DiscordSocketClient client)
        {
            _client = client;

            _commandService = new CommandService(new CommandServiceConfig()
            {
                DefaultRunMode = RunMode.Async,
                CaseSensitiveCommands = false,
                LogLevel = LogSeverity.Debug
            });

            _commandService.AddModulesAsync(Assembly.GetEntryAssembly()).Wait();

            _serviceProvider = ConfigureServiceProvider();
            _serviceProvider.Init();
            _prefixHandler = _serviceProvider.GetRequiredService<PrefixHandler>();

            _commandService.Log += CommandServiceLog;
            _client.MessageReceived += HandleCommand;
        }

        private Task CommandServiceLog(LogMessage arg)
        {
            Console.WriteLine(arg.ToString());
            return Task.CompletedTask;
        }

        private IServiceProvider ConfigureServiceProvider()
        {
            return new ServiceCollection()
                .AddDbContext<SQLiteContext>(options =>
                {
                    options.UseSqlite(Configuration.GetConnectionString("SQLite"));
                })
                .AddSingleton(_client)
                .AddSingleton(_commandService)
                .AddHandlers()
                .AddManagers()
                .AddServices()
                .AddLogging()
                .AddTools()
                .BuildServiceProvider();
        }

        private async Task HandleCommand(SocketMessage msg)
        {
            SocketUserMessage message = msg as SocketUserMessage;

            if (message is null || message.Author.IsBot)
                return;

            string stringPrefix = _prefixHandler.Get((message.Channel as SocketGuildChannel).Guild.Id);
            int argPos = 0;

            bool hasStringPrefix = message.HasStringPrefix(stringPrefix, ref argPos);
            bool isMentioned = message.HasMentionPrefix(_client.CurrentUser, ref argPos);

            if (!hasStringPrefix && !isMentioned)
                return;

            SocketCommandContext context = new SocketCommandContext(_client, message);
            IResult result = await _commandService.ExecuteAsync(context, argPos, _serviceProvider);

            if (!result.IsSuccess)
            {
                Console.WriteLine(new LogMessage(LogSeverity.Error, "Command", result.ErrorReason));
            }
        }
    }
}
