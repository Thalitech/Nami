﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using Newtonsoft.Json;

namespace Nami.Modules.Currency
{
    public class Cooldown
    {
        [JsonProperty("libs")]
        List<CooldownData> libs = new List<CooldownData>();
        internal static async Task<Cooldown> LoadConfigAsync()
        {
            var config = new Cooldown();
            var config_file = "Resources/config_cooldow.json";
            if(!File.Exists(config_file))
            {
                // Save new config file 
                var json = JsonConvert.SerializeObject(config, Formatting.Indented);
                await File.WriteAllTextAsync(config_file, json);
                return config;
            }
            else 
            {
                // Load config file 
                var json = await File.ReadAllTextAsync(config_file);
                config = JsonConvert.DeserializeObject<Cooldown>(json);
            }
            return config;
        }

        public CooldownData Find(DiscordGuild guild, DiscordMember? member) 
        {
            if (libs != null  && libs.Count > 0 && libs.Find(x => x.id == member.Id) != null)
                return libs.Find(x => x.gid == guild.Id && x.id == member.Id);
            else 
            {
                var cd = new CooldownData();
                cd.gid = guild.Id;
                cd.id = member.Id;
                cd.date = DateTime.Today;
                cd.date.AddDays(1);
                cd.created = DateTime.Today;
                libs.Add(cd);
                Save();
                return libs.Find(x => x.id == member.Id);
            }
        }
        internal void Add(DiscordGuild guild, DiscordMember member)
        {
            var cd = new CooldownData();
            cd.gid = guild.Id;
            cd.id = member.Id;
            cd.date = DateTime.Today;
            cd.date.AddDays(1);
            cd.created = DateTime.Today;
            libs.Add(cd);
            Save();
        }

        internal void Save()
        {
            var config_file = "Resources/config_cooldow.json";
            // Save new config file 
            var json = JsonConvert.SerializeObject(this, Formatting.Indented);
            File.WriteAllTextAsync(config_file, json);
        }

    }
    public class CooldownData
    {
        [JsonProperty("gid")]
        public ulong gid { get; set; }

        [JsonProperty("id")]
        public ulong id { get; set; }
        [JsonProperty("date")]
        public DateTime date { get; set; } = DateTime.Now;
        [JsonProperty("created")]
        internal DateTime created { get; set; } = DateTime.Today;
        
        internal bool IsTomorrow() => date.Day < DateTime.Now.Day;

        internal void AddDay()
        {
            throw new NotImplementedException();
        }
    }
}