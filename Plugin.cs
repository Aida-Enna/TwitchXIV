using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using System;
using TwitchXIV.Logging;
using TwitchXIV.Other;

namespace TwitchXIV
{
    public class Plugin : IDalamudPlugin
    {
        #region Variables

        [PluginService] public static IDalamudPluginInterface PluginInterface { get; set; } = null!;
        [PluginService] public static IPluginLog PluginLogger { get; set; } = null!;

        public static ILogger Logger { get; set; } = null!;
        public static WOLClient TwitchClient {  get; set; } = null!;
        public static string Name => "Twitch XIV (Reimagined)";
        public static Configuration PluginConfig { get; set; } = null!;

        private readonly PluginCommandManager<Plugin> CommandManager = null!;
        private readonly ConfigUi ConfigUi = null!;
        private readonly ChatUi ChatUi = null!;

        #endregion Variables
        #region Init/Deinit

        public Plugin(IDalamudPluginInterface pluginInterface, ICommandManager commands, IPluginLog pluginLogger)
        {
            PluginInterface = pluginInterface;
            PluginLogger = pluginLogger;
            PluginConfig = PluginInterface.GetPluginConfig() as Configuration ?? new();

            ChatUi = new();
            TwitchClient = new();
            ConfigUi = new();

            Logger = new ChatLogger(ChatUi);

            CommandManager = new PluginCommandManager<Plugin>(this, commands);

            PluginInterface.UiBuilder.Draw += ChatUi.Draw;
            PluginInterface.UiBuilder.Draw += ConfigUi.Draw;
            PluginInterface.UiBuilder.OpenMainUi += ChatUi.ToggleVisibility;
            PluginInterface.UiBuilder.OpenConfigUi += ConfigUi.ToggleVisibility;

            TwitchClient.Connect();
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);

            if (TwitchClient.IsConnected)
            {
                TwitchClient.Disconnect();
            }

            PluginInterface.UiBuilder.OpenConfigUi -= ConfigUi.ToggleVisibility;
            PluginInterface.UiBuilder.OpenMainUi -= ChatUi.ToggleVisibility;
            PluginInterface.UiBuilder.Draw -= ConfigUi.Draw;
            PluginInterface.UiBuilder.Draw -= ChatUi.Draw;

            CommandManager.Dispose();
            TwitchClient.Dispose();

            PluginInterface.SavePluginConfig(PluginConfig);


        }

        #endregion Init/Deinit
        #region Commands

        [Command("/twchat")]
        [Aliases("/twc")]
        [HelpMessage("Toggles Twitch chat window")]
        public void ToggleTwitchChat(string command, string args)
        {
            ChatUi.ToggleVisibility();
        }

        [Command("/twitch")]
        [Aliases("/twconfig")]
        [HelpMessage("Shows TwitchXIV configuration options")]
        public void ToggleTwitchOptions(string command, string args)
        {
            ConfigUi.ToggleVisibility();
        }

        [Command("/toff")]
        [HelpMessage("Disconnect from Twitch")]
        public void DisconnectFromTwitch(string command, string args)
        {
            if (TwitchClient.IsConnected)
            {
                TwitchClient.Disconnect();
                Plugin.Logger.Print("You have left the channel.");
            }
            else
            {
                Plugin.Logger.Print("You are not currently connected!");
            }
        }

        [Command("/tt")]
        [HelpMessage("Turn twitch chat relay on/off")]
        public void ToggleTwitch(string command, string args)
        {
            PluginConfig.TwitchEnabled = !PluginConfig.TwitchEnabled;
            Plugin.Logger.Print($"Toggled twitch chat {(PluginConfig.TwitchEnabled ? "on" : "off")}.");
        }

        [Command("/tw")]
        [HelpMessage("Sends a message to the specified channel in options\nUsage: /tw Hey guys, how is the stream going?")]
        public void SendTwitchChat(string command, string args)
        {
            if (!Plugin.PluginConfig.TwitchEnabled)
            {
                Plugin.Logger.Print("Twitch chat relay is currently disabled.");
                return;
            }
            if(String.IsNullOrWhiteSpace(args))
            {
                Plugin.Logger.Print("Error: No message specified");
                Plugin.Logger.Print("Usage: /tw Hey, how is the stream going?");
                return;
            }
            if (!TwitchClient.IsConnected)
            {
                Plugin.Logger.Print("You are not currently connected!");
                return;
            }
            if (TwitchClient.JoinedChannels.Count == 0)
            {
                Plugin.Logger.Print("You are not currently connected to a channel.");
                return;
            }
            TwitchClient.SendMessage(TwitchClient.JoinedChannels[0], args);
        }

        [Command("/tchannel")]
        [HelpMessage("Switch chat to the specified channel\nUsage: /tchannel streamer_username")]
        public void SwitchTwitchChannel(string command, string args)
        {
            try
            {
                if (String.IsNullOrWhiteSpace(args))
                {
                    Plugin.Logger.Print("Error: No channel specified.");
                    Plugin.Logger.Print("Usage: /tchannel streamer_username");
                    return;
                }
                if (TwitchClient.JoinedChannels.Count > 0) 
                { 
                    TwitchClient.LeaveChannel(TwitchClient.JoinedChannels[0]); 
                }
                PluginConfig.ChannelToSend = args;
                if (!TwitchClient.IsConnected)
                {
                    Plugin.Logger.Print("Connecting to Twitch...");
                    TwitchClient.Connect();
                }
                TwitchClient.JoinChannel(args);
                if (TwitchClient.JoinedChannels.Count > 0)
                {
                    Plugin.Logger.Print($"Channel joined!");
                }
                else
                {
                    Plugin.Logger.Print("Channel join failed!");
                }
            }
            catch(Exception f)
            {
                Plugin.Logger.Print(f.Message);
            }
        }

        #endregion Commands
    }
}