﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using ScrubBot.Database;
using ScrubBot.Database.Models;

namespace ScrubBot.Modules
{
    public class SettingsModule : ModuleBase<SocketCommandContext>
    {
        private CommandService _commandService;

        public SettingsModule(CommandService commandService) => Initialize(commandService);

        private void Initialize(CommandService commandService) => _commandService = commandService;

        [Command("Info"), Alias("BotInfo"), Summary("Display info about the bot.")]
        public async Task Info()
        {
            DatabaseContext db = new DatabaseContext();

            if (!GetGuild(db, out Guild guild))
            {
                EmbedBuilder errorEmbed = new EmbedBuilder { Color = Color.Red, Title = "ERROR", Description = "Current guild was not found in the database...\nAborting operation" };
                await ReplyAsync("", false, errorEmbed.Build());
                return;
            }

            EmbedBuilder embed = new EmbedBuilder { Color = Color.Purple, Title = "Bot Info" };
            embed.AddField("Server:", (guild.Name ?? "null") + "\n");

            if (guild.AuditChannelId != null)
            {
                var auditChannel = Context.Guild.GetChannel(Convert.ToUInt64(guild.AuditChannelId)) as SocketTextChannel;
                embed.AddField("Audit Channel:", (auditChannel != null ? auditChannel.Mention : "Invalid channel!") + "\n");
            }
            else
            {
                embed.AddField("Audit Channel:", "null\n");
            }

            embed.AddField("Char prefix:", (guild.CharPrefix != null ? $"' {guild.CharPrefix} '" : "null") + "\n");
            embed.AddField("String prefix:", (guild.StringPrefix != null ? $"'{guild.StringPrefix}'" : "null") + "\n");

            await ReplyAsync("", false, embed.Build());
        }

        [Command("Help")]
        public async Task Help()
        {
            List<CommandInfo> commands = _commandService.Commands.ToList();
            EmbedBuilder embed = new EmbedBuilder { Color = Color.Purple, Title = "Command list" };

            foreach (var command in commands)
            {
                if (command.Name == "Help") continue;
                
                if (command.Module.Name == typeof(OwnerModule).Name) continue;

                string embedFieldText = command.Summary;

                if (command.Parameters.Count > 0)
                    embedFieldText = command.Parameters.Aggregate(embedFieldText, (current, param) => current + $"\nParameters:\t{param.Type.Name} {param}\t");

                embed.AddField($"{command.Name} ({command.Module.Name.Replace("Module", "")})", embedFieldText);
            }

            await ReplyAsync("", false, embed.Build());
        }

        private bool GetGuild(DatabaseContext dbContext, out Guild outGuild)
        {
            string guildId = Context.Guild.Id.ToString();
            Guild localGuild = dbContext.Guilds.FirstOrDefault(x => x.Id == guildId);

            if (localGuild == null)
            {
                outGuild = null;
                return false;
            }

            outGuild = localGuild;
            return true;
        }

        private async Task OnGuildNotFound() => await ReplyAsync($"```Current guild was not found in the database...\nAborting operation```");
    }
}