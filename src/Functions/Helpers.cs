using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Utils;
using Discord;
using Discord.WebSocket;
using System.Text;
using System.Text.RegularExpressions;

namespace DiscordUtilities
{
    public partial class DiscordUtilities
    {
        private ServerData? serverData;
        private Dictionary<CCSPlayerController, PlayerData> playerData = new Dictionary<CCSPlayerController, PlayerData>();
        public class ServerData
        {
            public required string GameDirectory { get; set; }
            public required string Name { get; set; }
            public required string MaxPlayers { get; set; }
            public required string MapName { get; set; }
            public required string OnlinePlayers { get; set; }
            public required string OnlinePlayersAndBots { get; set; }
            public required string OnlineBots { get; set; }
            public required string Timeleft { get; set; }
        }

        public class PlayerData
        {
            public required string Name { get; set; }
            public required string NameWithoutEmoji { get; set; }
            public required string SteamId32 { get; set; }
            public required string SteamId64 { get; set; }
            public required string IpAddress { get; set; }
            public required string CommunityUrl { get; set; }
            public required string TeamShortName { get; set; }
            public required string TeamLongName { get; set; }
            public required string TeamNumber { get; set; }
            public required string Kills { get; set; }
            public required string Deaths { get; set; }
            public required string Assists { get; set; }
            public required string Points { get; set; }
            public required string CountryShort { get; set; }
            public required string CountryLong { get; set; }
            public required string CountryEmoji { get; set; }
            public required string DiscordGlobalname { get; set; }
            public required string DiscordDisplayName { get; set; }
            public required string DiscordPing { get; set; }
            public required string DiscordID { get; set; }
            public required bool IsLinked { get; set; }
        }

        public enum EmbedTypes
        {
            Report,
            Report_Reply,
            LinkFailed,
            LinkSuccess,
            AlreadyLinked,
            All_Chatlog,
            Team_Chatlog,
            Admin_Chatlog,
            ServerStatus,
            ServerStatus_Player,
            Connect,
            Disconnect,
            MapChanged,
            Rcon,
        }
        public enum ContentTypes
        {
            Report,
            Report_Reply,
            LinkFailed,
            LinkSuccess,
            AlreadyLinked,
            All_Chatlog,
            Team_Chatlog,
            Admin_Chatlog,
            ServerStatus,
            ServerStatus_Player,
            Connect,
            Disconnect,
            MapChanged,
            Rcon,
        }

        public string GetContent(ContentTypes type, string[] data)
        {
            string content = string.Empty;
            switch (type)
            {
                case ContentTypes.Connect:
                    content = ReplacePlayerDataVariables(Config.EventNotifications.Connect.ConnectedEmbed.Content, ulong.Parse(data[0]));
                    content = ReplaceServerDataVariables(content);
                    break;

                case ContentTypes.Disconnect:
                    content = ReplacePlayerDataVariables(Config.EventNotifications.Disconnect.DisconnectdEmbed.Content, ulong.Parse(data[0]));
                    content = ReplaceServerDataVariables(content);
                    break;

                case ContentTypes.MapChanged:
                    content = ReplaceServerDataVariables(Config.EventNotifications.MapChanged.MapChangedEmbed.Content);
                    break;

                case ContentTypes.Rcon:
                    content = Config.Rcon.RconReplyEmbed.Content;
                    content = content.Replace("{COMMAND}", data[0]);
                    content = content.Replace("{SERVER}", data[1]);
                    break;

                case ContentTypes.Report:
                    content = ReplacePlayerDataVariables(Config.Report.ReportEmbed.Content, ulong.Parse(data[0]));
                    content = ReplacePlayerDataVariables(content, ulong.Parse(data[1]), true);
                    content = ReplaceServerDataVariables(content);
                    content = content.Replace("{REASON}", data[2]);
                    break;

                case ContentTypes.Report_Reply:
                    content = Config.Report.ReportEmbed.ReportButton.ReplyReportEmbed.Content;
                    break;

                case ContentTypes.All_Chatlog:
                    content = ReplacePlayerDataVariables(Config.Chatlog.AllChatEmbed.Content, ulong.Parse(data[0]));
                    content = ReplaceServerDataVariables(content);
                    content = content.Replace("{MESSAGE}", data[1]);
                    break;

                case ContentTypes.Admin_Chatlog:
                    content = ReplacePlayerDataVariables(Config.Chatlog.AdminChat.AdminChatEmbed.Content, ulong.Parse(data[0]));
                    content = ReplaceServerDataVariables(content);
                    content = content.Replace("{MESSAGE}", data[1]);
                    break;

                case ContentTypes.Team_Chatlog:
                    content = ReplacePlayerDataVariables(Config.Chatlog.TeamChatEmbed.Content, ulong.Parse(data[0]));
                    content = ReplaceServerDataVariables(content);
                    content = content.Replace("{MESSAGE}", data[1]);
                    break;

                case ContentTypes.ServerStatus_Player:
                    content = ReplacePlayerDataVariables(Config.ServerStatus.ServerStatusEmbed.ServerStatusDropdown.ServerStatusDropdownClick.Content, ulong.Parse(data[0]));
                    content = ReplaceServerDataVariables(content);
                    break;

                case ContentTypes.ServerStatus:
                    content = ReplaceServerDataVariables(Config.ServerStatus.ServerStatusEmbed.Content);
                    break;

                case ContentTypes.LinkSuccess:
                    content = Config.Link.LinkEmbed.Success.Content.Replace("{STEAM}", data[0]);
                    break;

                case ContentTypes.LinkFailed:
                    content = Config.Link.LinkEmbed.Failed.Content.Replace("{CODE}", data[0]);
                    break;

                case ContentTypes.AlreadyLinked:
                    content = Config.Link.LinkEmbed.AlreadyLinked.Content.Replace("{STEAM}", data[0]);
                    break;

            }
            return content;
        }

        public EmbedBuilder GetEmbed(EmbedTypes type, string[] data)
        {
            var embed = new EmbedBuilder();
            var config = GetEmbedConfig(type);

            foreach (var prop in config.GetType().GetProperties())
            {
                var value = prop.GetValue(config);
                if (value != null && value.GetType() == typeof(bool))
                {
                    if (prop.Name == "FooterTimestamp" && (bool)value)
                        embed.WithCurrentTimestamp();
                }
                if (value != null && value.GetType() == typeof(string))
                {
                    string replacedValue = string.Empty;
                    switch (type)
                    {
                        case EmbedTypes.Connect:
                            replacedValue = ReplacePlayerDataVariables((string)value, ulong.Parse(data[0]));
                            replacedValue = ReplaceServerDataVariables(replacedValue);
                            break;

                        case EmbedTypes.Disconnect:
                            replacedValue = ReplacePlayerDataVariables((string)value, ulong.Parse(data[0]));
                            replacedValue = ReplaceServerDataVariables(replacedValue);
                            break;

                        case EmbedTypes.MapChanged:
                            replacedValue = ReplaceServerDataVariables((string)value);
                            break;

                        case EmbedTypes.Rcon:
                            replacedValue = (string)value;
                            replacedValue = replacedValue.Replace("{COMMAND}", data[0]);
                            replacedValue = replacedValue.Replace("{SERVER}", data[1]);
                            break;

                        case EmbedTypes.ServerStatus_Player:
                            replacedValue = ReplacePlayerDataVariables((string)value, ulong.Parse(data[0]));
                            replacedValue = ReplaceServerDataVariables(replacedValue);
                            break;

                        case EmbedTypes.ServerStatus:
                            replacedValue = ReplaceServerDataVariables((string)value);
                            break;

                        case EmbedTypes.Report:
                            replacedValue = ReplacePlayerDataVariables((string)value, ulong.Parse(data[0]));
                            replacedValue = ReplacePlayerDataVariables(replacedValue, ulong.Parse(data[1]), true);
                            replacedValue = ReplaceServerDataVariables(replacedValue);
                            replacedValue = replacedValue.Replace("{REASON}", data[2]);
                            break;

                        case EmbedTypes.Report_Reply:
                            replacedValue = (string)value;
                            break;

                        case EmbedTypes.All_Chatlog:
                            replacedValue = ReplacePlayerDataVariables((string)value, ulong.Parse(data[0]));
                            replacedValue = ReplaceServerDataVariables(replacedValue);
                            replacedValue = replacedValue.Replace("{MESSAGE}", data[1]);
                            break;

                        case EmbedTypes.Team_Chatlog:
                            replacedValue = ReplacePlayerDataVariables((string)value, ulong.Parse(data[0]));
                            replacedValue = ReplaceServerDataVariables(replacedValue);
                            replacedValue = replacedValue.Replace("{MESSAGE}", data[1]);
                            break;

                        case EmbedTypes.Admin_Chatlog:
                            replacedValue = ReplacePlayerDataVariables((string)value, ulong.Parse(data[0]));
                            replacedValue = ReplaceServerDataVariables(replacedValue);
                            replacedValue = replacedValue.Replace("{MESSAGE}", data[1]);
                            break;

                        case EmbedTypes.LinkSuccess:
                            replacedValue = (string)value;
                            replacedValue = replacedValue.Replace("{STEAM}", data[0]);
                            break;
                        case EmbedTypes.LinkFailed:
                            replacedValue = (string)value;
                            replacedValue = replacedValue.Replace("{CODE}", data[0]);
                            break;

                        case EmbedTypes.AlreadyLinked:
                            replacedValue = (string)value;
                            replacedValue = replacedValue.Replace("{STEAM}", data[0]);
                            break;
                    }
                    SetEmbedProperty(embed, prop.Name, replacedValue);
                }
            }
            return embed;
        }

        private void SetEmbedProperty(EmbedBuilder embed, string propertyName, string value)
        {
            if (string.IsNullOrEmpty(value))
                return;

            switch (propertyName)
            {
                case "Title":
                    embed.WithTitle(value);
                    break;
                case "Description":
                    embed.WithDescription(value);
                    break;
                case "Thumbnail":
                    if (value.Contains(".jpg") || value.Contains(".png") || value.Contains(".gif"))
                        embed.WithThumbnailUrl(value);
                    break;
                case "Image":
                    if (value.Contains(".jpg") || value.Contains(".png") || value.Contains(".gif"))
                        embed.WithImageUrl(value);
                    break;
                case "Color":
                    if (value.StartsWith("#"))
                        value = value.Substring(1);
                    embed.WithColor(new Color(Convert.ToUInt32(value, 16)));
                    break;
                case "Footer":
                    embed.WithFooter(value);
                    break;
                case "Fields":
                    string[] fields = value.Split('|');
                    foreach (var field in fields)
                    {
                        string[] fieldData = field.Split(';');
                        if (fieldData.Length == 3)
                            embed.AddField(fieldData[0], fieldData[1], bool.Parse(fieldData[2]));
                        else
                        {
                            SendConsoleMessage($"[Discord Utilities] Invalid Fields Format! ({value})", ConsoleColor.DarkRed);
                            return;
                        }
                    }
                    break;
                default:
                    break;
            }
        }

        private object GetEmbedConfig(EmbedTypes type)
        {
            return type switch
            {
                EmbedTypes.ServerStatus => Config.ServerStatus.ServerStatusEmbed,
                EmbedTypes.ServerStatus_Player => Config.ServerStatus.ServerStatusEmbed.ServerStatusDropdown.ServerStatusDropdownClick,
                EmbedTypes.Report => Config.Report.ReportEmbed,
                EmbedTypes.Report_Reply => Config.Report.ReportEmbed.ReportButton.ReplyReportEmbed,
                EmbedTypes.LinkSuccess => Config.Link.LinkEmbed.Success,
                EmbedTypes.LinkFailed => Config.Link.LinkEmbed.Failed,
                EmbedTypes.AlreadyLinked => Config.Link.LinkEmbed.AlreadyLinked,
                EmbedTypes.Team_Chatlog => Config.Chatlog.TeamChatEmbed,
                EmbedTypes.All_Chatlog => Config.Chatlog.AllChatEmbed,
                EmbedTypes.Admin_Chatlog => Config.Chatlog.AdminChat.AdminChatEmbed,
                EmbedTypes.Connect => Config.EventNotifications.Connect.ConnectedEmbed,
                EmbedTypes.Disconnect => Config.EventNotifications.Disconnect.DisconnectdEmbed,
                EmbedTypes.MapChanged => Config.EventNotifications.MapChanged.MapChangedEmbed,
                EmbedTypes.Rcon => Config.Rcon.RconReplyEmbed,
                _ => null!,
            };
        }

        private CCSPlayerController GetTargetBySteamID64(ulong steamid)
        {
            foreach (var p in Utilities.GetPlayers().Where(p => p != null && p.IsValid && p.SteamID.ToString().Length == 17 && p.AuthorizedSteamID != null))
            {
                if (p.AuthorizedSteamID!.SteamId64 == steamid)
                    return p;
            }
            return null!;
        }

        private CCSPlayerController GetTargetByName(string name, CCSPlayerController player)
        {
            int matchingCount = 0;
            CCSPlayerController target = null!;

            foreach (var p in Utilities.GetPlayers().Where(p => p.IsValid && p.SteamID.ToString().Length == 17 && !AdminManager.PlayerHasPermissions(p, Config.Report.UnreportableFlag)))
            {
                if (p.PlayerName.Contains(name))
                {
                    target = p;
                    matchingCount++;
                }
            }

            if (matchingCount == 0)
            {
                if (player == null)
                    SendConsoleMessage($"[Discord Utilities] Player with name '{name}' not found!", ConsoleColor.DarkYellow);
                else
                    player.PrintToChat($"{Localizer["Chat.Prefix"]} {Localizer["Chat.TargetNotFound", name]}");
                return null!;
            }
            if (matchingCount > 1)
            {
                if (player == null)
                    SendConsoleMessage($"[Discord Utilities] Multiple players with name '{name}' found!", ConsoleColor.DarkYellow);
                else
                    player.PrintToChat($"{Localizer["Chat.Prefix"]} {Localizer["Chat.MultipleTargetFound", name]}");
                return null!;
            }

            return target;
        }
        /*private CCSPlayerController GetTargetByUserId(int userid)
        {
            return Utilities.GetPlayers().Where(p => p.IsValid && p.UserId == userid && p.Connected == PlayerConnectedState.PlayerConnected && p.SteamID.ToString().Length == 17).FirstOrDefault() ?? null!;
        }*/

        private string GetRandomCode(int length)
        {
            string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            var keyBuilder = new StringBuilder(length);

            for (int i = 0; i < length; i++)
            {
                keyBuilder.Append(chars[random.Next(chars.Length)]);
            }

            return keyBuilder.ToString();
        }

        private string ReplacePlayerDataVariables(string replacedString, ulong steamid, bool isTarget = false)
        {
            var target = GetTargetBySteamID64(steamid);
            if (target == null)
                return replacedString;

            PlayerData? selectedPlayer = null;
            if (playerData.ContainsKey(target))
                selectedPlayer = playerData[target];
            else
                return replacedString;

            string player = isTarget ? "Target" : "Player";
            if (selectedPlayer != null)
            {
                var replacedData = new Dictionary<string, string>
                {
                    { $"{{{player}.Name}}", selectedPlayer.Name},
                    { $"{{{player}.NameWithoutEmoji}}", selectedPlayer.NameWithoutEmoji},
                    { $"{{{player}.SteamID32}}", selectedPlayer.SteamId32},
                    { $"{{{player}.SteamID64}}", selectedPlayer.SteamId64},
                    { $"{{{player}.IpAddress}}", selectedPlayer.IpAddress},
                    { $"{{{player}.CommunityUrl}}", selectedPlayer.CommunityUrl},
                    { $"{{{player}.TeamShortName}}", selectedPlayer.TeamShortName},
                    { $"{{{player}.TeamLongName}}", selectedPlayer.TeamLongName},
                    { $"{{{player}.TeamNumber}}", selectedPlayer.TeamNumber },
                    { $"{{{player}.Kills}}", selectedPlayer.Kills},
                    { $"{{{player}.Deaths}}", selectedPlayer.Deaths},
                    { $"{{{player}.Assists}}", selectedPlayer.Assists},
                    { $"{{{player}.Points}}", selectedPlayer.Points},
                    { $"{{{player}.CountryShort}}", selectedPlayer.CountryShort},
                    { $"{{{player}.CountryLong}}", selectedPlayer.CountryLong},
                    { $"{{{player}.CountryEmoji}}", selectedPlayer.CountryEmoji},
                    { $"{{{player}.DiscordGlobalname}}", selectedPlayer.DiscordGlobalname},
                    { $"{{{player}.DiscordDisplayName}}", selectedPlayer.DiscordDisplayName},
                    { $"{{{player}.DiscordPing}}", selectedPlayer.DiscordPing},
                    { $"{{{player}.DiscordID}}", selectedPlayer.DiscordID}
                };

                foreach (var item in replacedData)
                {
                    if (replacedString.Contains(item.Key))
                    {
                        replacedString = replacedString.Replace(item.Key, item.Value);
                    }
                }
            }

            return replacedString;
        }

        private string GetTeamShortName(int team)
        {
            switch (team)
            {
                case 1:
                    return "Spec";
                case 2:
                    return "T";
                case 3:
                    return "CT";
                default:
                    return "None";
            }
        }
        private string GetTeamLongName(int team)
        {
            switch (team)
            {
                case 1:
                    return "Spectator";
                case 2:
                    return "Terrorist";
                case 3:
                    return "Counter-Terrorist";
                default:
                    return "None";
            }
        }
        private string ReplaceServerDataVariables(string replacedString)
        {
            var replacedData = new Dictionary<string, string>
            {
                { "{Server.Name}", serverData!.Name },
                { "{Server.MaxPlayers}", serverData.MaxPlayers },
                { "{Server.MapName}", serverData.MapName },
                { "{Server.Timeleft}", serverData.Timeleft },
                { "{Server.OnlinePlayers}", serverData.OnlinePlayers },
                { "{Server.OnlinePlayersAndBots}", serverData.OnlinePlayersAndBots },
                { "{Server.OnlineBots}", serverData.OnlineBots },
                { "{Server.IP}", Config.ServerIP }
            };

            foreach (var item in replacedData)
            {
                if (replacedString.Contains(item.Key))
                {
                    replacedString = replacedString.Replace(item.Key, item.Value);
                }
            }
            return replacedString;
        }
        private string ReplaceDiscordUserVariables(SocketGuildUser user, string replacedString)
        {
            var replacedData = new Dictionary<string, string>
            {
                { "{Discord.UserDisplayName}", user.DisplayName },
                { "{Discord.UserGlobalName}", user.GlobalName },
                { "{Discord.UserID}", user.Id.ToString() }
            };
            foreach (var item in replacedData)
            {
                if (replacedString.Contains(item.Key))
                    replacedString = replacedString.Replace(item.Key, item.Value);
            }
            return replacedString;
        }
        private string ReplaceDiscordChannelVariables(SocketMessage message, string replacedString)
        {
            var replacedData = new Dictionary<string, string>
            {
                { "{Discord.ChannelName}", message.Channel.Name },
                { "{Discord.ChannelID}", message.Channel.Id.ToString() },
                { "{Discord.Message}", message.Content }
            };
            foreach (var item in replacedData)
            {
                if (replacedString.Contains(item.Key))
                    replacedString = replacedString.Replace(item.Key, item.Value);
            }
            return replacedString;
        }
        private string ReplaceColors(string message)
        {
            var modifiedValue = message;
            foreach (var field in typeof(ChatColors).GetFields())
            {
                string pattern = $"{{{field.Name}}}";
                if (modifiedValue.Contains(pattern, StringComparison.OrdinalIgnoreCase))
                {
                    modifiedValue = modifiedValue.Replace(pattern, field.GetValue(null)?.ToString() ?? string.Empty, StringComparison.OrdinalIgnoreCase);
                }
            }
            return modifiedValue.Equals(message) ? message : $" {modifiedValue}";
        }
        private bool IsEmbedValid(EmbedBuilder Embed)
        {
            int embedOptions = 0;
            if (!string.IsNullOrEmpty(Embed.Title))
                embedOptions++;
            if (!string.IsNullOrEmpty(Embed.Description))
                embedOptions++;
            if (Embed.Fields.Count > 0)
                embedOptions++;
            if (Embed.Footer != null)
                embedOptions++;
            if (!string.IsNullOrEmpty(Embed.ImageUrl))
                embedOptions++;
            if (!string.IsNullOrEmpty(Embed.ThumbnailUrl))
                embedOptions++;

            return embedOptions > 0;
        }

        private static string IsValidFlag(string flagEmoji)
        {
            var flagsList = new List<string>
            {
                ":flag_ac:", ":flag_ad:", ":flag_ae:", ":flag_af:", ":flag_ag:",
                ":flag_ai:", ":flag_al:", ":flag_am:", ":flag_ao:", ":flag_aq:",
                ":flag_ar:", ":flag_as:", ":flag_at:", ":flag_au:", ":flag_aw:",
                ":flag_ax:", ":flag_az:", ":flag_ba:", ":flag_bb:", ":flag_bd:",
                ":flag_be:", ":flag_bf:", ":flag_bg:", ":flag_bh:", ":flag_bi:",
                ":flag_bj:", ":flag_bl:", ":flag_bm:", ":flag_bn:", ":flag_bo:",
                ":flag_bq:", ":flag_br:", ":flag_bs:", ":flag_bt:", ":flag_bv:",
                ":flag_bw:", ":flag_by:", ":flag_bz:", ":flag_ca:", ":flag_cc:",
                ":flag_cd:", ":flag_cf:", ":flag_cg:", ":flag_ch:", ":flag_ci:",
                ":flag_ck:", ":flag_cl:", ":flag_cm:", ":flag_cn:", ":flag_co:",
                ":flag_cp:", ":flag_cr:", ":flag_cu:", ":flag_cv:", ":flag_cw:",
                ":flag_cx:", ":flag_cy:", ":flag_cz:", ":flag_de:", ":flag_dg:",
                ":flag_dj:", ":flag_dk:", ":flag_dm:", ":flag_do:", ":flag_dz:",
                ":flag_ea:", ":flag_ec:", ":flag_ee:", ":flag_eg:", ":flag_eh:",
                ":flag_er:", ":flag_es:", ":flag_et:", ":flag_eu:", ":flag_fi:",
                ":flag_fj:", ":flag_fk:", ":flag_fm:", ":flag_fo:", ":flag_fr:",
                ":flag_ga:", ":flag_gb:", ":flag_gd:", ":flag_ge:", ":flag_gf:",
                ":flag_gg:", ":flag_gh:", ":flag_gi:", ":flag_gl:", ":flag_gm:",
                ":flag_gn:", ":flag_gp:", ":flag_gq:", ":flag_gr:", ":flag_gs:",
                ":flag_gt:", ":flag_gu:", ":flag_gw:", ":flag_gy:", ":flag_hk:",
                ":flag_hm:", ":flag_hn:", ":flag_hr:", ":flag_ht:", ":flag_hu:",
                ":flag_ic:", ":flag_id:", ":flag_ie:", ":flag_il:", ":flag_im:",
                ":flag_in:", ":flag_io:", ":flag_iq:", ":flag_ir:", ":flag_is:",
                ":flag_it:", ":flag_je:", ":flag_jm:", ":flag_jo:", ":flag_jp:",
                ":flag_ke:", ":flag_kg:", ":flag_kh:", ":flag_ki:", ":flag_km:",
                ":flag_kn:", ":flag_kp:", ":flag_kr:", ":flag_kw:", ":flag_ky:",
                ":flag_kz:", ":flag_la:", ":flag_lb:", ":flag_lc:", ":flag_li:",
                ":flag_lk:", ":flag_lr:", ":flag_ls:", ":flag_lt:", ":flag_lu:",
                ":flag_lv:", ":flag_ly:", ":flag_ma:", ":flag_mc:", ":flag_md:",
                ":flag_me:", ":flag_mf:", ":flag_mg:", ":flag_mh:", ":flag_mk:",
                ":flag_ml:", ":flag_mm:", ":flag_mn:", ":flag_mo:", ":flag_mp:",
                ":flag_mq:", ":flag_mr:", ":flag_ms:", ":flag_mt:", ":flag_mu:",
                ":flag_mv:", ":flag_mw:", ":flag_mx:", ":flag_my:", ":flag_mz:",
                ":flag_na:", ":flag_nc:", ":flag_ne:", ":flag_nf:", ":flag_ng:",
                ":flag_ni:", ":flag_nl:", ":flag_no:", ":flag_np:", ":flag_nr:",
                ":flag_nu:", ":flag_nz:", ":flag_om:", ":flag_pa:", ":flag_pe:",
                ":flag_pf:", ":flag_pg:", ":flag_ph:", ":flag_pk:", ":flag_pl:",
                ":flag_pm:", ":flag_pn:", ":flag_pr:", ":flag_ps:", ":flag_pt:",
                ":flag_pw:", ":flag_py:", ":flag_qa:", ":flag_re:", ":flag_ro:",
                ":flag_rs:", ":flag_ru:", ":flag_rw:", ":flag_sa:", ":flag_sb:",
                ":flag_sc:", ":flag_sd:", ":flag_se:", ":flag_sg:", ":flag_sh:",
                ":flag_si:", ":flag_sj:", ":flag_sk:", ":flag_sl:", ":flag_sm:",
                ":flag_sn:", ":flag_so:", ":flag_sr:", ":flag_ss:", ":flag_st:",
                ":flag_sv:", ":flag_sx:", ":flag_sy:", ":flag_sz:", ":flag_ta:",
                ":flag_tc:", ":flag_td:", ":flag_tf:", ":flag_tg:", ":flag_th:",
                ":flag_tj:", ":flag_tk:", ":flag_tl:", ":flag_tm:", ":flag_tn:",
                ":flag_to:", ":flag_tr:", ":flag_tt:", ":flag_tv:", ":flag_tw:",
                ":flag_tz:", ":flag_ua:", ":flag_ug:", ":flag_um:", ":flag_us:",
                ":flag_uy:", ":flag_uz:", ":flag_va:", ":flag_vc:", ":flag_ve:",
                ":flag_vg:", ":flag_vi:", ":flag_vn:", ":flag_vu:", ":flag_wf:",
                ":flag_ws:", ":flag_xk:", ":flag_ye:", ":flag_yt:", ":flag_za:",
                ":flag_zm:", ":flag_zw:"
            };

            if (flagsList.Contains(flagEmoji))
                return flagEmoji;

            return ":flag_white:";
        }
        private static string RemoveEmoji(string text)
        {
            return Regex.Replace(text, @"[\uD83C-\uDBFF\uDC00-\uDFFF]+", string.Empty);
        }
        private int GetPlayersCount()
        {
            return Utilities.GetPlayers().Where(p => p.IsValid && !p.IsHLTV && !p.IsBot && p.Connected == PlayerConnectedState.PlayerConnected && p.SteamID.ToString().Length == 17).Count();
        }
        private int GetPlayersCountWithBots()
        {
            return Utilities.GetPlayers().Where(p => p.IsValid && !p.IsHLTV && p.Connected == PlayerConnectedState.PlayerConnected).Count();
        }
        private int GetBotsCounts()
        {
            return Utilities.GetPlayers().Where(p => p.IsValid && !p.IsHLTV && p.IsBot).Count();
        }
        private int GetTargetsForReportCount(CCSPlayerController player)
        {
            return Utilities.GetPlayers().Where(p => p.IsValid && p != player && !p.IsHLTV && !p.IsBot && p.Connected == PlayerConnectedState.PlayerConnected && p.SteamID.ToString().Length == 17 && !AdminManager.PlayerHasPermissions(p, Config.Report.UnreportableFlag)).Count();
        }
        private static CCSGameRules GameRules()
        {
            return Utilities.FindAllEntitiesByDesignerName<CCSGameRulesProxy>("cs_gamerules").First().GameRules!;
        }
    }
}