using Dalamud.Game;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Hooking;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using System;
using System.Linq;
using Veda;

namespace TwitchXIV
{
    public class Plugin : IDalamudPlugin
    {
        public string Name => "Twitch XIV";

        [PluginService] public static IDalamudPluginInterface PluginInterface { get; set; }
        [PluginService] public static ISigScanner SigScanner { get; set; }
        [PluginService] public static IChatGui Chat { get; set; }
        [PluginService] public static IPartyList PartyList { get; set; }

        //[PluginService] public static IClientState ClientState { get; set; }
        //[PluginService] public static IGameInteropProvider GameInteropProvider { get; set; }

        public static Configuration PluginConfig { get; set; }
        private PluginCommandManager<Plugin> CommandManager;
        private PluginUI ui;

        public static bool FirstRun = true;
        public string PreviousWorkingChannel;
        public bool SuccessfullyJoined;
        //private Hook<Rapture.Delegates.AddMsgSourceEntry>? ContentIdResolverHook { get; init; }


        public unsafe Plugin(IDalamudPluginInterface pluginInterface, IChatGui chat, IPartyList partyList, ICommandManager commands, ISigScanner sigScanner)
        {
            PluginInterface = pluginInterface;
            PartyList = partyList;
            Chat = chat;
            SigScanner = sigScanner;

            // Get or create a configuration object
            PluginConfig = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
            PluginConfig.Initialize(PluginInterface);
           
            ui = new PluginUI();
            PluginInterface.UiBuilder.Draw += new System.Action(ui.Draw);
            PluginInterface.UiBuilder.OpenConfigUi += () =>
            {
                PluginUI ui = this.ui;
                ui.IsVisible = !ui.IsVisible;
            };

            // Load all of our commands
            CommandManager = new PluginCommandManager<Plugin>(this, commands);
            //Chat.ChatMessage += OnChatMessage;

            //ContentIdResolverHook = Plugin.GameInteropProvider.HookFromAddress<RaptureLogModule.Delegates.AddMsgSourceEntry>(RaptureLogModule.MemberFunctionPointers.AddMsgSourceEntry, ContentIdResolver);
            //ContentIdResolverHook.Enable();

            try
            {
                if (PluginConfig.Username != "Your twitch.tv username" && PluginConfig.OAuthCode.Length == 36)
                {
                    WOLClient.DoConnect();
                }
                else
                {
                    Chat.Print(Functions.BuildSeString(PluginInterface.InternalName, "Please open the config with <c575>/twitch and set your credentials."));
                }
            }
            catch(Exception f)
            {
                Chat.PrintError("Something went wrong - " + f.Message.ToString());
                Chat.Print(Functions.BuildSeString(PluginInterface.InternalName, "Please open the config with <c575>/twitch and double check your credentials."));
            }
        }

        //Aida, don't forget to uncomment the disposal of this method too
        //private void OnChatMessage(Dalamud.Game.Text.XivChatType type, int timestamp, ref Dalamud.Game.Text.SeStringHandling.SeString sender, ref Dalamud.Game.Text.SeStringHandling.SeString message, ref bool isHandled)
        //{
        //    if (isHandled)
        //    {
        //        return;
        //    }
        //    var ChatMessage = message.Payloads.FirstOrDefault(x => x is TextPayload) as TextPayload;
        //    if (PluginConfig.TwitchOnlyMode && sender.TextValue == ClientState.LocalPlayer.Name.TextValue.ToString())
        //    {
        //        Chat.Print("a");
        //        if (String.IsNullOrWhiteSpace(ChatMessage.Text))
        //        {
        //            Chat.PrintError("Error: No message specified");
        //            Chat.Print(Functions.BuildSeString(PluginInterface.InternalName, "Usage: /tw Hey guys, how is the stream going?", ColorType.Warn));
        //            return;
        //        }
        //        if (WOLClient.Client.IsConnected == false)
        //        {
        //            Chat.Print(Functions.BuildSeString(PluginInterface.InternalName, "You are not currently connected to a channel.", ColorType.Twitch));
        //            return;
        //        }
        //        WOLClient.Client.SendMessage(WOLClient.Client.JoinedChannels.First(), ChatMessage.Text);
        //        isHandled = true;
        //        return;
        //    }
        //}

        [Command("/twitch")]
        [Aliases("/twconfig")]
        [HelpMessage("Shows TwitchXIV configuration options")]
        public void ShowTwitchOptions(string command, string args)
        {
            ui.IsVisible = !ui.IsVisible;
        }

        //[Command("/twmode")]
        //[HelpMessage("Toggles Twitch-only chat mode")]
        //public void ToggleTwitchChatMode(string command, string args)
        //{
        //    PluginConfig.TwitchOnlyMode  = !PluginConfig.TwitchOnlyMode;
        //    Chat.Print($"Toggled twitch mode {(PluginConfig.TwitchOnlyMode ? "on" : "off")}.");
        //}

        [Command("/toff")]
        [HelpMessage("Disconnect from Twitch")]
        public void DisconnectFromTwitch(string command, string args)
        {
            if (WOLClient.Client.IsConnected)
            {
                WOLClient.Client.Disconnect();
                Chat.Print(Functions.BuildSeString(PluginInterface.InternalName, "You have left the channel.", ColorType.Twitch));
            }
            else
            {
                Chat.Print(Functions.BuildSeString(PluginInterface.InternalName, "You are not currently connected!", ColorType.Warn));
            }
        }

        [Command("/tt")]
        [HelpMessage("Turn twitch chat relay on/off")]
        public void ToggleTwitch(string command, string args)
        {
            PluginConfig.TwitchEnabled = !PluginConfig.TwitchEnabled;
            Chat.Print($"Toggled twitch chat {(PluginConfig.TwitchEnabled ? "on" : "off")}.");
        }

        [Command("/tw")]
        [HelpMessage("Sends a message to the specified channel in options\nUsage: /tw Hey guys, how is the stream going?")]
        public void SendTwitchChat(string command, string args)
        {
            if(String.IsNullOrWhiteSpace(args))
            {
                Chat.PrintError("Error: No message specified");
                Chat.Print(Functions.BuildSeString(PluginInterface.InternalName, "Usage: /tw Hey guys, how is the stream going?", ColorType.Warn));
                return;
            }
            if (WOLClient.Client.IsConnected == false)
            {
                Chat.Print(Functions.BuildSeString(PluginInterface.InternalName, "You are not currently connected to a channel.", ColorType.Twitch));
                return;
            }
            WOLClient.Client.SendMessage(WOLClient.Client.JoinedChannels.First(), args);
        }

        [Command("/tchannel")]
        [HelpMessage("Switch chat to the specified channel\nUsage: /tchannel streamer_username")]
        public void SwitchTwitchChannel(string command, string args)
        {
            try
            {
                if (String.IsNullOrWhiteSpace(args))
                {
                    Chat.PrintError("Error: No channel specified");
                    Chat.Print(Functions.BuildSeString(PluginInterface.InternalName, "Usage: /tchannel streamer_username\nExample: /tchannel zackrawrr", ColorType.Warn));
                    return;
                }
                if (WOLClient.Client.JoinedChannels.Count() > 0) { WOLClient.Client.LeaveChannel(WOLClient.Client.JoinedChannels.First()); }
                PluginConfig.ChannelToSend = args;
                if (WOLClient.Client.IsConnected == false)
                {
                    Chat.Print(Functions.BuildSeString(PluginInterface.InternalName, "Connecting to Twitch...", ColorType.Twitch));
                    WOLClient.DoConnect();
                }
                WOLClient.Client.JoinChannel(args);
            }
            catch(Exception f)
            {
                Chat.PrintError(f.ToString());
            }
        }
        #region IDisposable Support

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing) return;

            CommandManager.Dispose();

            PluginInterface.SavePluginConfig(PluginConfig);

            PluginInterface.UiBuilder.Draw -= ui.Draw;
            PluginInterface.UiBuilder.OpenConfigUi -= () =>
            {
                PluginUI ui = this.ui;
                ui.IsVisible = !ui.IsVisible;
            };

            if (WOLClient.Client.IsConnected) { WOLClient.Client.Disconnect(); }
            WOLClient.Client.OnLog -= WOLClient.Client_OnLog;
            WOLClient.Client.OnJoinedChannel -= WOLClient.Client_OnJoinedChannel;
            WOLClient.Client.OnLeftChannel -= WOLClient.Client_OnLeftChannel;
            WOLClient.Client.OnMessageSent -= WOLClient.Client_OnMessageSent;
            WOLClient.Client.OnMessageReceived -= WOLClient.Client_OnMessageReceived;
            //Chat.ChatMessage -= OnChatMessage;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion IDisposable Support
    }
}