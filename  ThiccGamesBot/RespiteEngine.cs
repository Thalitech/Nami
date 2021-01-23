using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Nami
{
    public class RespiteEngine
    {
        public static List<RespiteMember> members = new List<RespiteMember>();
        internal static async Task Add(DiscordGuild server, DiscordMember member, string reason)
        {
            var _curMember = members.Find(x => x.member.Id == member.Id);
            if(_curMember != null)
            {
                await _curMember.AddTime(reason);
            }
            else
            {
                members.Add(new RespiteMember(server, member, reason));
            }
        }
        internal static async Task Remove(DiscordGuild guild, DiscordMember member, string reasone)
        {
            var _curMember = members.Find(x => x.member.Id == member.Id);
            if (_curMember != null)
            {
                await _curMember.ClearTime();
            }
        }
    }

    public class RespiteMember
    {
        public DiscordGuild server { get; private set; }
        public DiscordMember member { get; private set; }

        private string reason;
        private double lastTimeoutTime;
        private double seconds;

        public double timeoutTime { get; private set; }

        private Thread workingThread = null;
        private bool cancellation;

        public RespiteMember(DiscordGuild server, DiscordMember member, string reason)
        {
            this.server = server;
            this.member = member;
            this.reason = reason;
            this.timeoutTime = TimeSpan.FromMinutes(3).TotalMinutes;
            this.lastTimeoutTime = timeoutTime;
            workingThread = new Thread(() => TimedFunction().ConfigureAwait(false));
            workingThread.Start();
            member.SendMessageAsync($"You have been put into timeout fro {timeoutTime}m by an admin in {server.Name}. reasone: {reason}").ConfigureAwait(false);
        }
        internal async Task AddTime(string reason)
        {
            this.reason = reason;
            await member.SendMessageAsync($"Your timeout is not +2 in {server.Name}. reasone: {reason}");
            seconds = TimeSpan.FromMinutes(TimeSpan.FromSeconds(seconds).Minutes + 2).TotalSeconds;
        }

        internal async Task ClearTime()
        {
            timeoutTime = 0; cancellation = true;
            await member.SendMessageAsync($"You have been remove out of timeout by and admin in {server.Name}. reasone: {reason}");
        }

        private async Task TimedFunction(double time = default)
        {
            this.timeoutTime = TimeSpan.FromMinutes(time == default ? 3 : time).TotalMinutes;
            this.lastTimeoutTime = timeoutTime;

            seconds = TimeSpan.FromMinutes(timeoutTime).TotalSeconds;
            // Mute The member
            await member.SetMuteAsync(true, $"You have been put into timeout by an admin in {server.Name}. reasone: {reason}");
            while (seconds > 0)
            {
                Thread.Sleep(1000);
                seconds--;
                if (cancellation)
                {
                    seconds = 0;
                    timeoutTime = 0;
                    break;
                }
            }
            timeoutTime = 0;
            cancellation = false;
            await member.SendMessageAsync($"Your time is up, you may interact with the {server.Name} as normal now.");
            // Unmute the member
            await member.SetMuteAsync(false, $"Your time is up, you may interact with the {server.Name} as normal now.");
        }

    }
}