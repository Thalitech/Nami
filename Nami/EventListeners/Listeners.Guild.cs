﻿using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.DependencyInjection;
using Nami.Database;
using Nami.EventListeners.Attributes;
using Nami.EventListeners.Common;
using Nami.Extensions;
using Nami.Modules.Administration.Services;
using Nami.Services;
using Nami.Services.Common;

namespace Nami.EventListeners
{
    internal static partial class Listeners
    {
        [AsyncEventListener(DiscordEventType.GuildAvailable)]
        public static async Task OnGuildAvailable(NamiBot bot, GuildCreateEventArgs e)
        {
            new Thread(async () => {
                DiscordServer.servers.Add(new DiscordServer(e.Guild));
                Console.WriteLine($"{e.Guild.Name} is now registered ● {DateTime.Now}");
                await GuildUpdater(e.Guild);
                await Task.CompletedTask;
            }).Start();
            await Task.CompletedTask;
        }
       
        [AsyncEventListener(DiscordEventType.GuildMemberAdded)]
        public static Task OnGuildMemberAdded(NamiBot bot, GuildMemberAddEventArgs e)
        {
            new Thread(async () => {
                var channel = e.Guild.Channels.ToList().Find(x => x.Value.Name.ToLower().Contains("announce")).Value;
                if (channel == null) channel = e.Guild.SystemChannel;
                var embed = new DiscordEmbedBuilder();
                embed.Timestamp = new DateTimeOffset(DateTime.Now);
                embed.Title = "Member Join";
                embed.Color = DiscordColor.Green;
                embed.Description = $"@everyone {e.Member.Mention} has joined, lest welcome them! :smile:";
                embed.Footer = new DiscordEmbedBuilder.EmbedFooter() {
                    Text = $"Thank you for your time. {e.Guild.Name} Team",
                };
                await channel.SendMessageAsync(embed: embed.Build());
            }).Start();
            return Task.CompletedTask;
        }
       
        [AsyncEventListener(DiscordEventType.GuildMemberRemoved)]
        public static Task OnGuildMemberRemoved(NamiBot bot, GuildMemberRemoveEventArgs e)
        {
            new Thread(async () => {
                var channel = e.Guild.Channels.ToList().Find(x => x.Value.Name.ToLower().Contains("announce")).Value;
                if (channel == null) channel = e.Guild.SystemChannel;
                var embed = new DiscordEmbedBuilder();
                embed.Timestamp = new DateTimeOffset(DateTime.Now);
                embed.Title = "Member Left";
                embed.Color = DiscordColor.Red;
                embed.Description = $"{e.Member.DisplayName} has left, so sad to see you go :cry:";
                embed.Footer = new DiscordEmbedBuilder.EmbedFooter() {
                    Text = $"Thank you for your time. {e.Guild.Name} Team",
                };
                await channel.SendMessageAsync(embed: embed.Build());
            }).Start();
            return Task.CompletedTask;
        }



        [AsyncEventListener(DiscordEventType.GuildBanAdded)]
        public static async Task GuildBanEventHandlerAsync(NamiBot bot, GuildBanAddEventArgs e)
        {
            LogExt.Debug(bot.GetId(e.Guild.Id), "Banned: {Member} {Guild}", e.Member, e.Guild);
            if (!LoggingService.IsLogEnabledForGuild(bot, e.Guild.Id, out LoggingService logService, out LocalizedEmbedBuilder emb))
                return;

            emb.WithLocalizedTitle(DiscordEventType.GuildBanAdded, "evt-gld-ban-add");

            DiscordAuditLogBanEntry? entry = await e.Guild.GetLatestAuditLogEntryAsync<DiscordAuditLogBanEntry>(AuditLogActionType.Ban);
            emb.WithDescription(e.Member);
            emb.AddFieldsFromAuditLogEntry(entry);
            await logService.LogAsync(e.Guild, emb);
        }

        [AsyncEventListener(DiscordEventType.GuildBanRemoved)]
        public static async Task GuildUnbanEventHandlerAsync(NamiBot bot, GuildBanRemoveEventArgs e)
        {
            LogExt.Debug(bot.GetId(e.Guild.Id), "Unanned: {Member} {Guild}", e.Member, e.Guild);
            if (!LoggingService.IsLogEnabledForGuild(bot, e.Guild.Id, out LoggingService logService, out LocalizedEmbedBuilder emb))
                return;

            emb.WithLocalizedTitle(DiscordEventType.GuildBanAdded, "evt-gld-ban-del");

            DiscordAuditLogBanEntry? entry = await e.Guild.GetLatestAuditLogEntryAsync<DiscordAuditLogBanEntry>(AuditLogActionType.Unban);
            emb.WithDescription(e.Member);
            emb.AddFieldsFromAuditLogEntry(entry);
            await logService.LogAsync(e.Guild, emb);
        }

        [AsyncEventListener(DiscordEventType.GuildDeleted)]
        public static Task GuildDeleteEventHandlerAsync(NamiBot bot, GuildDeleteEventArgs e)
        {
            LogExt.Information(bot.GetId(e.Guild.Id), "Guild deleted {Guild}", e.Guild);
            return bot.Services.GetRequiredService<GuildConfigService>().UnregisterGuildAsync(e.Guild.Id);
        }

        [AsyncEventListener(DiscordEventType.GuildEmojisUpdated)]
        public static async Task GuildEmojisUpdateEventHandlerAsync(NamiBot bot, GuildEmojisUpdateEventArgs e)
        {
            LogExt.Information(bot.GetId(e.Guild.Id), "Emojis updated {Guild}, ({EmojisBefore} to {EmojisAfter}", e.Guild, e.EmojisBefore, e.EmojisAfter);
            if (!LoggingService.IsLogEnabledForGuild(bot, e.Guild.Id, out LoggingService logService, out LocalizedEmbedBuilder emb))
                return;

            AuditLogActionType action = e.EmojisAfter.Count > e.EmojisBefore.Count
                ? AuditLogActionType.EmojiCreate
                : e.EmojisAfter.Count < e.EmojisBefore.Count
                    ? AuditLogActionType.EmojiDelete
                    : AuditLogActionType.EmojiUpdate;

            emb.WithLocalizedTitle(DiscordEventType.GuildBanAdded, "evt-gld-emoji-chn", desc: null, action);

            DiscordAuditLogEmojiEntry? entry = await e.Guild.GetLatestAuditLogEntryAsync<DiscordAuditLogEmojiEntry>(action);
            emb.AddFieldsFromAuditLogEntry(entry, (emb, ent) => {
                if (ent.Target is null) {
                    emb.WithLocalizedDescription("evt-gld-emoji-unk");
                    return;
                }
                emb.WithThumbnail(ent.Target.Url);
                switch (action) {
                    case AuditLogActionType.EmojiCreate:
                        emb.AddLocalizedTitleField("evt-gld-emoji-add", ent.Target.Name, inline: true);
                        break;
                    case AuditLogActionType.EmojiDelete:
                        emb.AddLocalizedTitleField("evt-gld-emoji-del", ent.NameChange.Before, inline: true);
                        break;
                    case AuditLogActionType.EmojiUpdate:
                        emb.AddLocalizedPropertyChangeField("str-name", ent.NameChange, inline: true);
                        break;
                }
            });

            if (entry is null) {
                emb.AddLocalizedTitleField("evt-gld-emoji-bef", e.EmojisBefore?.Count, inline: true);
                emb.AddLocalizedTitleField("evt-gld-emoji-aft", e.EmojisAfter?.Count, inline: true);
            }

            await logService.LogAsync(e.Guild, emb);
        }

        [AsyncEventListener(DiscordEventType.GuildIntegrationsUpdated)]
        public static async Task GuildIntegrationsUpdateEventHandlerAsync(NamiBot bot, GuildIntegrationsUpdateEventArgs e)
        {
            LogExt.Debug(bot.GetId(e.Guild.Id), "Integrations updated: {Guild}", e.Guild);
            if (!LoggingService.IsLogEnabledForGuild(bot, e.Guild.Id, out LoggingService logService, out LocalizedEmbedBuilder emb))
                return;

            emb.WithLocalizedTitle(DiscordEventType.GuildIntegrationsUpdated, "evt-gld-int-upd");
            await logService.LogAsync(e.Guild, emb);
        }

        [AsyncEventListener(DiscordEventType.GuildRoleCreated)]
        public static async Task GuildRoleCreateEventHandlerAsync(NamiBot bot, GuildRoleCreateEventArgs e)
        {
            LogExt.Debug(bot.GetId(e.Guild.Id), "Role created: {Role} {Guild}", e.Role, e.Guild);
            if (!LoggingService.IsLogEnabledForGuild(bot, e.Guild.Id, out LoggingService logService, out LocalizedEmbedBuilder emb))
                return;

            emb.WithLocalizedTitle(DiscordEventType.GuildRoleCreated, "evt-gld-role-add", e.Role);
            emb.AddLocalizedPropertyChangeField("str-managed", false, e.Role?.IsManaged ?? false, inline: true);

            DiscordAuditLogRoleUpdateEntry? entry = await e.Guild.GetLatestAuditLogEntryAsync<DiscordAuditLogRoleUpdateEntry>(AuditLogActionType.RoleCreate);
            emb.AddFieldsFromAuditLogEntry(entry);
            await logService.LogAsync(e.Guild, emb);
        }

        [AsyncEventListener(DiscordEventType.GuildRoleDeleted)]
        public static async Task GuildRoleDeleteEventHandlerAsync(NamiBot bot, GuildRoleDeleteEventArgs e)
        {
            LogExt.Debug(bot.GetId(e.Guild.Id), "Role deleted: {Role} {Guild}", e.Role, e.Guild);
            if (!LoggingService.IsLogEnabledForGuild(bot, e.Guild.Id, out LoggingService logService, out LocalizedEmbedBuilder emb))
                return;

            emb.WithLocalizedTitle(DiscordEventType.GuildRoleDeleted, "evt-gld-role-del", e.Role);

            DiscordAuditLogRoleUpdateEntry? entry = await e.Guild.GetLatestAuditLogEntryAsync<DiscordAuditLogRoleUpdateEntry>(AuditLogActionType.RoleDelete);
            emb.AddFieldsFromAuditLogEntry(entry);
            await logService.LogAsync(e.Guild, emb);
        }

        [AsyncEventListener(DiscordEventType.GuildRoleUpdated)]
        public static async Task GuildRoleUpdateEventHandlerAsync(NamiBot bot, GuildRoleUpdateEventArgs e)
        {
            LogExt.Debug(bot.GetId(e.Guild.Id), "Role updated: {Role} {Guild}", e.RoleBefore, e.Guild);
            // FIXME Still no solution for Discord role position event spam...
            if (OnlyPositionUpdate(e.RoleBefore, e.RoleAfter))
                return;

            if (!LoggingService.IsLogEnabledForGuild(bot, e.Guild.Id, out LoggingService logService, out LocalizedEmbedBuilder emb))
                return;

            emb.WithLocalizedTitle(DiscordEventType.GuildRoleUpdated, "evt-gld-role-upd", e.RoleBefore);

            DiscordAuditLogRoleUpdateEntry? entry = await e.Guild.GetLatestAuditLogEntryAsync<DiscordAuditLogRoleUpdateEntry>(AuditLogActionType.RoleUpdate);
            emb.AddFieldsFromAuditLogEntry(entry, (emb, ent) => {
                emb.AddLocalizedPropertyChangeField("str-name", ent.NameChange);
                emb.AddLocalizedPropertyChangeField("str-color", ent.ColorChange);
                emb.AddLocalizedPropertyChangeField("str-hoist", ent.HoistChange);
                emb.AddLocalizedPropertyChangeField("str-mention", ent.MentionableChange);
                emb.AddLocalizedPropertyChangeField("str-pos", ent.PositionChange);
                // TODO use permissions_new once it is implemented in D#+
                emb.AddLocalizedTitleField("str-perms", ent.PermissionChange.After, inline: false);
            });

            await logService.LogAsync(e.Guild, emb);


            static bool OnlyPositionUpdate(DiscordRole roleBefore, DiscordRole roleAfter)
            {
                return roleBefore.Position != roleAfter.Position
                    && roleBefore.Color.Value == roleAfter.Color.Value
                    && roleBefore.IsHoisted == roleAfter.IsHoisted
                    && roleBefore.IsManaged == roleAfter.IsManaged
                    && roleBefore.IsMentionable == roleAfter.IsMentionable
                    && roleBefore.Name == roleAfter.Name
                    && roleBefore.Permissions == roleAfter.Permissions;
            }
        }

        [AsyncEventListener(DiscordEventType.GuildUpdated)]
        public static async Task GuildUpdateEventHandlerAsync(NamiBot bot, GuildUpdateEventArgs e)
        {
            LogExt.Debug(bot.GetId(e.GuildBefore.Id), "Guild updated: {Guild}", e.GuildBefore);
            if (!LoggingService.IsLogEnabledForGuild(bot, e.GuildBefore.Id, out LoggingService logService, out LocalizedEmbedBuilder emb))
                return;

            emb.WithLocalizedTitle(DiscordEventType.GuildUpdated, "evt-gld-upd");

            DiscordAuditLogGuildEntry? entry = await e.GuildAfter.GetLatestAuditLogEntryAsync<DiscordAuditLogGuildEntry>(AuditLogActionType.GuildUpdate);
            emb.AddFieldsFromAuditLogEntry(entry, (emb, ent) => {
                emb.AddLocalizedPropertyChangeField("str-name", ent.NameChange);
                emb.AddLocalizedPropertyChangeField("str-afkchn", ent.AfkChannelChange);
                emb.AddLocalizedPropertyChangeField("str-embchn", ent.EmbedChannelChange);
                emb.AddLocalizedPropertyChangeField("str-nsfw", ent.ExplicitContentFilterChange);
                emb.AddLocalizedPropertyChangeField("str-icon", ent.IconChange);
                emb.AddLocalizedPropertyChangeField("str-mfa", ent.MfaLevelChange);
                emb.AddLocalizedPropertyChangeField("str-notifications", ent.NotificationSettingsChange);
                emb.AddLocalizedPropertyChangeField("str-owner", ent.OwnerChange);
                emb.AddLocalizedPropertyChangeField("str-region", ent.RegionChange);
                emb.AddLocalizedPropertyChangeField("str-splash", ent.SplashChange);
                emb.AddLocalizedPropertyChangeField("str-syschn", ent.SystemChannelChange);
                emb.AddLocalizedPropertyChangeField("str-verlvl", ent.VerificationLevelChange);
            });

            await logService.LogAsync(e.GuildAfter, emb);
        }

        [AsyncEventListener(DiscordEventType.VoiceServerUpdated)]
        public static async Task GuildVoiceServerUpdateEventHandlerAsync(NamiBot bot, VoiceServerUpdateEventArgs e)
        {
            LogExt.Debug(bot.GetId(e.Guild.Id), "Guild voice server updated: {Endpoint} {Guild}", e.Endpoint, e.Guild);
            if (!LoggingService.IsLogEnabledForGuild(bot, e.Guild.Id, out LoggingService logService, out LocalizedEmbedBuilder emb))
                return;

            emb.WithLocalizedTitle(DiscordEventType.VoiceServerUpdated, "evt-gld-vs-upd", e.Endpoint);
            await logService.LogAsync(e.Guild, emb);
        }

        [AsyncEventListener(DiscordEventType.InviteCreated)]
        public static Task InviteCreatedEventHandlerAsync(NamiBot bot, InviteCreateEventArgs e)
        {
            if (e.Guild is null)
                return Task.CompletedTask;

            LogExt.Debug(bot.GetId(e.Guild.Id), "Guild invite created: {Invite} {Guild}", e.Invite, e.Guild);
            if (!LoggingService.IsLogEnabledForGuild(bot, e.Guild.Id, out LoggingService logService, out LocalizedEmbedBuilder emb))
                return Task.CompletedTask;

            emb.WithLocalizedTitle(DiscordEventType.InviteCreated, "evt-gld-inv-add", e.Channel);
            emb.AddLocalizedTitleField("str-code", e.Invite.Code, inline: true);
            emb.AddLocalizedTitleField("str-revoked", e.Invite.IsRevoked, inline: true);
            emb.AddLocalizedTitleField("str-temporary", e.Invite.IsTemporary, inline: true);
            emb.AddLocalizedTitleField("str-max-age-s", e.Invite.MaxAge, inline: true);
            emb.AddLocalizedTitleField("str-max-uses", e.Invite.MaxUses, inline: true);
            if (e.Invite.Inviter is { })
                emb.AddInvocationFields(e.Invite.Inviter, e.Channel);
            return logService.LogAsync(e.Guild, emb);
        }

        [AsyncEventListener(DiscordEventType.InviteDeleted)]
        public static async Task InviteDeletedEventHandlerAsync(NamiBot bot, InviteDeleteEventArgs e)
        {
            LogExt.Debug(bot.GetId(e.Guild.Id), "Guild invite deleted: {Invite} {Guild}", e.Invite, e.Guild);
            if (!LoggingService.IsLogEnabledForGuild(bot, e.Guild.Id, out LoggingService logService, out LocalizedEmbedBuilder emb))
                return;

            LocalizationService ls = bot.Services.GetRequiredService<LocalizationService>();
            emb.WithLocalizedTitle(DiscordEventType.InviteDeleted, "evt-gld-inv-add", e.Channel);
            emb.AddLocalizedTitleField("str-code", e.Invite.Code, inline: true);
            emb.AddLocalizedTitleField("str-revoked", e.Invite.IsRevoked, inline: true);
            emb.AddLocalizedTitleField("str-temporary", e.Invite.IsTemporary, inline: true);
            emb.AddLocalizedTitleField("str-max-age-s", e.Invite.MaxAge, inline: true);
            emb.AddLocalizedTitleField("str-max-uses", e.Invite.MaxUses, inline: true);
            emb.WithLocalizedFooter("str-created-at", ls.GetLocalizedTimeString(e.Guild.Id, e.Invite.CreatedAt), e.Invite.Inviter.AvatarUrl);
            await logService.LogAsync(e.Guild, emb);
        }



        private static Task GuildUpdater(DiscordGuild e)
        {
            new Thread(async () =>
            {
                update:
                var _members = await e.GetAllMembersAsync();
                var _channels = await e.GetChannelsAsync();
                var membercount = _members.Count;
                var mc = _members.ToList().FindAll(x => x.Presence?.Status == UserStatus.Online);
                var onlinecount = mc.Count;
                var memberchIndex = -1;
                var onlinechIndex = -1;
                for (int i = 0; i < _channels.Count; i++)
                {
                    if (_channels[i].Name.ToLower().StartsWith("members:")) memberchIndex = i;
                    if (_channels[i].Name.ToLower().StartsWith("online:")) onlinechIndex = i;
                }
                if (memberchIndex > -1)
                    await _channels[memberchIndex].ModifyAsync(a => { a.Name = $"Members: {membercount}"; });
                if (onlinechIndex > -1)
                    await _channels[onlinechIndex].ModifyAsync(a => { a.Name = $"Online: {onlinecount}"; });

                await Task.Delay(20000);
                goto update;
            }).Start();
            return Task.CompletedTask;
        }
    }
}
