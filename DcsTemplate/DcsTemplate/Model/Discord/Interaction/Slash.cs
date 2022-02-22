using DisCatSharp;
using DisCatSharp.ApplicationCommands;
using DisCatSharp.Entities;
using DisCatSharp.Enums;
using DisCatSharp.CommandsNext.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DcsTemplate.Model.Discord.Interaction
{
    /// <summary>
    /// The slash commands.
    /// </summary>
    internal class Slash : ApplicationCommandsModule
    {
        /// <summary>
        /// Send the help of this bot.
        /// </summary>
        /// <param name="interactionContext">The interaction context.</param>
        [SlashCommand("help", "Schattenclown Help", true)]
        public static async Task HelpAsync(InteractionContext interactionContext)
        {
            await interactionContext.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

            DiscordEmbedBuilder discordEmbedBuilder = new DiscordEmbedBuilder()
            {
                Title = "Help",
                Description = "This is the command help for the Schattenclown Bot",
                Color = DiscordColor.Purple
            };
            discordEmbedBuilder.AddField("/level", "Shows your level!");
            discordEmbedBuilder.AddField("/leaderboard", "Shows the levelsystem!");
            discordEmbedBuilder.AddField("/timer", "Set´s a timer!");
            discordEmbedBuilder.AddField("/mytimers", "Look up your timers!");
            discordEmbedBuilder.AddField("/alarmclock", "Set an alarm for a spesific time!");
            discordEmbedBuilder.AddField("/myalarms", "Look up your alarms!");
            discordEmbedBuilder.AddField("/invite", "Send´s an invite link!");
            discordEmbedBuilder.WithAuthor("Schattenclown help");
            discordEmbedBuilder.WithFooter("(✿◠‿◠) thanks for using me");
            discordEmbedBuilder.WithTimestamp(DateTime.Now);

            await interactionContext.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(discordEmbedBuilder.Build()));
        }

        /// <summary>
        /// Generates an Invite link.
        /// </summary>
        /// <param name="interactionContext">The ic.</param>
        /// <returns>A Task.</returns>
        [SlashCommand("invite", "Invite ListforgeNotify", true)]
        public static async Task InviteAsync(InteractionContext interactionContext)
        {
            await interactionContext.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

            var bot_invite = interactionContext.Client.GetInAppOAuth(Permissions.Administrator);

            await interactionContext.EditResponseAsync(new DiscordWebhookBuilder().WithContent(bot_invite.AbsoluteUri));
        }

        /// <summary>
        /// Generates an Invite link.
        /// </summary>
        /// <param name="interactionContext">The ic.</param>
        /// <returns>A Task.</returns>
        [SlashCommand("Poke", "Poke a user!", true)]
        public static async Task Poke(InteractionContext interactionContext, [Option("User", "@...")] DiscordUser discordUser)
        {
            DiscordMember discordMember = discordUser as DiscordMember;
            await PokeAsync(interactionContext, null, discordMember, false, 2, false);
        }

        [SlashCommand("ForcePoke", "Poke a user! Forcefully!", true)]
        public static async Task ForcePoke(InteractionContext interactionContext, [Option("User", "@...")] DiscordUser discordUser)
        {
            DiscordMember discordMember = discordUser as DiscordMember;
            await PokeAsync(interactionContext, null, discordMember, false, 2, true);
        }

        [ContextMenu(ApplicationCommandType.User, "Poke a user!", true)]
        public static async Task AppsPoke(ContextMenuContext contextMenuContext)
        {
            await PokeAsync(null, contextMenuContext, contextMenuContext.TargetMember, true, 2, false);
        }

        [ContextMenu(ApplicationCommandType.User, "Poke a user! Forcefully!", true)]
        public static async Task AppsForcePoke(ContextMenuContext contextMenuContext)
        {
            await PokeAsync(null, contextMenuContext, contextMenuContext.TargetMember, true, 2, true);
        }

        public static async Task PokeAsync(InteractionContext interactionContext, ContextMenuContext contextMenuContext, DiscordMember discordTargetMember, bool deleteResponseAsync, int pokeAmount, bool force)
        {
            if (interactionContext != null)
                await interactionContext.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
            else
                await contextMenuContext.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

            var discordEmbedBuilder = new DiscordEmbedBuilder
            {
                Title = $"Poke {discordTargetMember.DisplayName}"
            };

            if (interactionContext != null)
                discordEmbedBuilder.WithFooter($"Requested by {interactionContext.Member.DisplayName}", interactionContext.Member.AvatarUrl);
            else
                discordEmbedBuilder.WithFooter($"Requested by {contextMenuContext.Member.DisplayName}", contextMenuContext.Member.AvatarUrl);

            if (interactionContext != null)
                await interactionContext.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(discordEmbedBuilder.Build()));
            else
                await contextMenuContext.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(discordEmbedBuilder.Build()));

            bool rightToMove = false;
            List<DiscordRole> discordRoleList = new();

            if (interactionContext != null)
                discordRoleList = interactionContext.Member.Roles.ToList();
            else
                discordRoleList = contextMenuContext.Member.Roles.ToList();

            foreach (DiscordRole discordRoleItem in discordRoleList)
            {
                if (discordRoleItem.Permissions.HasPermission(Permissions.MoveMembers))
                    rightToMove = true;
            }

            bool desktopHasValue = false;
            bool webHasValue = false;
            bool mobileHasValue = false;
            bool presenceWasNull = false;

            if (discordTargetMember.Presence != null)
            {
                desktopHasValue = discordTargetMember.Presence.ClientStatus.Desktop.HasValue;
                webHasValue = discordTargetMember.Presence.ClientStatus.Web.HasValue;
                mobileHasValue = discordTargetMember.Presence.ClientStatus.Mobile.HasValue;
            }
            else
                presenceWasNull = true;

            if (discordTargetMember.VoiceState != null && rightToMove && (force || presenceWasNull || ((desktopHasValue || webHasValue) && !mobileHasValue)))
            {
                DiscordChannel currentChannel = default;
                DiscordChannel tempCategory = default;
                DiscordChannel tempChannel2 = default;
                DiscordChannel tempChannel1 = default;

                try
                {
                    DiscordEmoji discordEmoji = DiscordEmoji.FromName(DiscordBot.Client, ":no_entry_sign:");

                    if (interactionContext != null)
                    {
                        tempCategory = interactionContext.Guild.CreateChannelCategoryAsync("%Temp%").Result;
                        tempChannel1 = interactionContext.Guild.CreateVoiceChannelAsync(discordEmoji, tempCategory).Result;
                        tempChannel2 = interactionContext.Guild.CreateVoiceChannelAsync(discordEmoji, tempCategory).Result;
                    }
                    else
                    {
                        tempCategory = contextMenuContext.Guild.CreateChannelCategoryAsync("%Temp%").Result;
                        tempChannel1 = contextMenuContext.Guild.CreateVoiceChannelAsync(discordEmoji, tempCategory).Result;
                        tempChannel2 = contextMenuContext.Guild.CreateVoiceChannelAsync(discordEmoji, tempCategory).Result;
                    }
                }
                catch
                {
                    discordEmbedBuilder.Description = "Error while creating the channels!";
                    if (interactionContext != null)
                        await interactionContext.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(discordEmbedBuilder.Build()));
                    else
                        await contextMenuContext.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(discordEmbedBuilder.Build()));
                }

                try
                {
                    currentChannel = discordTargetMember.VoiceState.Channel;

                    for (int i = 0; i < pokeAmount; i++)
                    {
                        await discordTargetMember.ModifyAsync(x => x.VoiceChannel = tempChannel1);
                        await Task.Delay(250);
                        await discordTargetMember.ModifyAsync(x => x.VoiceChannel = tempChannel2);
                        await Task.Delay(250);
                    }
                    await discordTargetMember.ModifyAsync(x => x.VoiceChannel = tempChannel1);
                    await Task.Delay(250);
                }
                catch
                {
                    discordEmbedBuilder.Description = "Error! User left?";
                    if (interactionContext != null)
                        await interactionContext.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(discordEmbedBuilder.Build()));
                    else
                        await contextMenuContext.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(discordEmbedBuilder.Build()));
                }

                try
                {
                    await discordTargetMember.ModifyAsync(x => x.VoiceChannel = currentChannel);
                }
                catch
                {
                    discordEmbedBuilder.Description = "Error! User left?";
                    if (interactionContext != null)
                        await interactionContext.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(discordEmbedBuilder.Build()));
                    else
                        await contextMenuContext.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(discordEmbedBuilder.Build()));
                }

                try
                {
                    await tempChannel2.DeleteAsync();
                }
                catch
                {
                    discordEmbedBuilder.Description = "Error while deleting the channels!";
                    if (interactionContext != null)
                        await interactionContext.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(discordEmbedBuilder.Build()));
                    else
                        await contextMenuContext.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(discordEmbedBuilder.Build()));
                }

                try
                {
                    await tempChannel1.DeleteAsync();
                }
                catch
                {
                    discordEmbedBuilder.Description = "Error while deleting the channels!";
                    if (interactionContext != null)
                        await interactionContext.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(discordEmbedBuilder.Build()));
                    else
                        await contextMenuContext.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(discordEmbedBuilder.Build()));
                }

                try
                {
                    await tempCategory.DeleteAsync();
                }
                catch
                {
                    discordEmbedBuilder.Description = "Error while deleting the channels!";
                    if (interactionContext != null)
                        await interactionContext.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(discordEmbedBuilder.Build()));
                    else
                        await contextMenuContext.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(discordEmbedBuilder.Build()));
                }
            }
            else if (discordTargetMember.VoiceState == null)
            {
                discordEmbedBuilder.Description = "User is not connected!";
                if (interactionContext != null)
                    await interactionContext.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(discordEmbedBuilder.Build()));
                else
                    await contextMenuContext.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(discordEmbedBuilder.Build()));
            }
            else if (!rightToMove)
            {
                discordEmbedBuilder.Description = "Your not allowed to use that!";
                if (interactionContext != null)
                    await interactionContext.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(discordEmbedBuilder.Build()));
                else
                    await contextMenuContext.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(discordEmbedBuilder.Build()));
            }
            else if (mobileHasValue)
            {
                string description = "Their phone will explode STOP!\n";

                DiscordEmoji discordEmoji_white_check_mark = DiscordEmoji.FromName(DiscordBot.Client, ":white_check_mark:");
                DiscordEmoji discordEmojiCheck_x = DiscordEmoji.FromName(DiscordBot.Client, ":x:");

                if (discordTargetMember.Presence.ClientStatus.Desktop.HasValue)
                    description += discordEmoji_white_check_mark + " Dektop" + "\n";
                else
                    description += discordEmojiCheck_x + " Dektop" + "\n";

                if (discordTargetMember.Presence.ClientStatus.Web.HasValue)
                    description += discordEmoji_white_check_mark + " Web" + "\n";
                else
                    description += discordEmojiCheck_x + " Web" + "\n";

                if (discordTargetMember.Presence.ClientStatus.Mobile.HasValue)
                    description += discordEmoji_white_check_mark + " Mobile";
                else
                    description += discordEmojiCheck_x + " Mobile";

                discordEmbedBuilder.Description = description;

                if (interactionContext != null)
                    await interactionContext.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(discordEmbedBuilder.Build()));
                else
                    await contextMenuContext.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(discordEmbedBuilder.Build()));
            }

            if (deleteResponseAsync)
            {
                for (int i = 3; i > 0; i--)
                {
                    discordEmbedBuilder.AddField("This message will be deleted in", $"{i} Secounds");
                    await contextMenuContext.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(discordEmbedBuilder.Build()));
                    await Task.Delay(1000);
                    discordEmbedBuilder.RemoveFieldAt(0);
                }

                await contextMenuContext.DeleteResponseAsync();
            }
        }

        /*/// <summary>
        /// Gets the user's avatar & banner.
        /// </summary>
        /// <param name="contextMenuContext">The contextmenu context.</param>
        [ContextMenu(ApplicationCommandType.User, "Get avatar & banner")]
        public static async Task GetUserBannerAsync(ContextMenuContext contextMenuContext)
        {
            var user = await contextMenuContext.Client.GetUserAsync(contextMenuContext.TargetUser.Id, true);

            var discordEmbedBuilder = new DiscordEmbedBuilder
            {
                Title = $"Avatar & Banner of {user.Username}",
                ImageUrl = user.BannerHash != null ? user.BannerUrl : null
            }.
            WithThumbnail(user.AvatarUrl).
            WithColor(user.BannerColor ?? DiscordColor.Purple).
            WithFooter($"Requested by {contextMenuContext.Member.DisplayName}", contextMenuContext.Member.AvatarUrl).
            WithAuthor($"{user.Username}", user.AvatarUrl, user.AvatarUrl);
            await contextMenuContext.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(discordEmbedBuilder.Build()));
        }*/
    }
}
