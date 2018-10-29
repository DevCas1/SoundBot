﻿using ScrubBot.Database;
using ScrubBot.Domain;
using ScrubBot.Tools;

using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;

namespace ScrubBot.Managers
{
    public class PrefixManager : IPrefixManager
    {
        private readonly SQLiteContext _context;
        private readonly ConcurrentDictionary<ulong, string> _prefixes;

        public PrefixManager(SQLiteContext dbContext)
        {
            _context = dbContext;
            _prefixes = new ConcurrentDictionary<ulong, string>();

            var guilds = _context.Guilds.Select(x => new { x.Id, x.Prefix }).ToList();

            foreach (var guild in guilds)
            {
                _prefixes.TryAdd(guild.Id, guild.Prefix ?? Configuration.Get("Prefix:Default"));
            }
        }

        public string Get(ulong guildId)
        {
            bool hasValue = _prefixes.TryGetValue(guildId, out string value);
            return hasValue ? value : Configuration.Get("Prefix:Default");
        }

        public async Task<bool> SetAsync(ulong guildId, string prefix)
        {
            Guild guild = _context.Guilds.Find(guildId);
            guild.Prefix = prefix;
            _context.Guilds.Update(guild);

            await _context.SaveChangesAsync();
            _prefixes.AddOrUpdate(guildId, prefix, (key, oldValue) => prefix);
            return true;
        }
    }
}
