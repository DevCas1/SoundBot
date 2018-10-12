﻿using ScrubBot.Database;
using ScrubBot.Database.Models;

using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;

namespace ScrubBot.Handlers
{
    public class PrefixHandler
    {
        private readonly DatabaseContext _context;
        private readonly ConcurrentDictionary<ulong, string> _prefixes;

        public PrefixHandler(DatabaseContext dbContext)
        {
            _context = dbContext;
            _prefixes = new ConcurrentDictionary<ulong, string>();

            var Guilds = _context.Guilds.Select(x => new { x.Id, x.Prefix }).ToList();

            foreach (var guild in Guilds)
            {
                _prefixes.TryAdd(guild.Id, guild.Prefix);
            }
        }

        public string Get(ulong guildId)
        {
            bool hasValue = _prefixes.TryGetValue(guildId, out string value);
            return value;
        }

        public async Task<bool> SetAsync(ulong guildId, string prefix)
        {
            Guild guild = _context.Guilds.Find(guildId);
            guild.Prefix = prefix;
            _context.Guilds.Update(guild);

            await _context.SaveChangesAsync();
            return _prefixes.TryAdd(guildId, prefix);
        }
    }
}
