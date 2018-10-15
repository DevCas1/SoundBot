﻿using Discord.WebSocket;

using Microsoft.EntityFrameworkCore;

using ScrubBot.Database;
using ScrubBot.Domain;
using ScrubBot.Extensions;

using System.Linq;
using System.Threading.Tasks;

namespace ScrubBot.Managers
{
    public class UserManager
    {
        private readonly SQLiteContext _dbContext;
        private readonly DiscordSocketClient _client;

        public UserManager(SQLiteContext dbContext, DiscordSocketClient client)
        {
            _dbContext = dbContext;
            _client = client;

            _client.UserBanned += UserBannedAsync;
            _client.UserJoined += UserJoinedAsync;
            _client.UserLeft += UserLeftAsync;
            _client.UserUnbanned += UserUnbannedAsync;
            _client.UserUpdated += UserUpdatedAsync;
        }

        public async Task AddUserAsync(SocketGuildUser socketGuildUser)
        {
            if (_dbContext.Users.Any(x => x.Id == socketGuildUser.Id))
                return;

            User user = socketGuildUser.ToUser();
            user.Guild = ToGuild(socketGuildUser.Guild);

            await _dbContext.Users.AddAsync(user);
            await _dbContext.SaveChangesAsync();
        }

        public async Task RemoveUserAsync(SocketGuildUser user)
        {
            User userToRemove = await _dbContext.Users.FirstOrDefaultAsync(x => x.Id == user.Id);

            if (userToRemove is null)
                return;

            _dbContext.Users.Remove(userToRemove);
            await _dbContext.SaveChangesAsync();
        }

        private Guild ToGuild(SocketGuild socketGuild)
        {
            return _dbContext.Guilds.FirstOrDefault(x => x.Id == socketGuild.Id) ??
                   new Guild
                   {
                       Name = socketGuild.Name,
                       IconUrl = socketGuild.IconUrl,
                       Id = socketGuild.Id,
                       MemberCount = socketGuild.MemberCount
                   };
        }

        public async Task UserBannedAsync(SocketUser user, SocketGuild guild)
        {

            await Task.CompletedTask;
        }

        public async Task UserJoinedAsync(SocketGuildUser user)
        {

            await Task.CompletedTask;
        }

        public async Task UserLeftAsync(SocketGuildUser user)
        {

            await Task.CompletedTask;
        }

        public async Task UserUnbannedAsync(SocketUser user, SocketGuild guild)
        {

            await Task.CompletedTask;
        }

        public async Task UserUpdatedAsync(SocketUser before, SocketUser after)
        {

            await Task.CompletedTask;
        }
    }
}
