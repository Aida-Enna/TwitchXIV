using System;
using TwitchLib.Client.Models;
using TwitchLib.Client;
using TwitchLib.Communication.Models;
using TwitchLib.Communication.Clients;
using System.Text.RegularExpressions;
using TwitchLib.Client.Events;
using TwitchLib.Communication.Events;
using System.Collections.Generic;

namespace TwitchXIV
{
    public class WOLClient : IDisposable
    {
        #region Variables

        public bool IsConnected { get => Client.IsConnected; }
        public IReadOnlyList<JoinedChannel> JoinedChannels { get => Client.JoinedChannels; }

        private TwitchClient Client = null!;

        #endregion Variables
        #region Init/Deinit

        public WOLClient()
        {
            Client = new(new WebSocketClient(new ClientOptions()
            {
                MessagesAllowedInPeriod = 750,
                ThrottlingPeriod = TimeSpan.FromSeconds(30)
            }));

            Client.OnLog += OnLog;
            Client.OnJoinedChannel += OnJoinedChannel;
            Client.OnLeftChannel += OnLeftChannel;
            Client.OnMessageSent += OnMessageSent;
            Client.OnMessageReceived += OnMessageReceived;
            Client.OnError += OnError;
            //Client.OnWhisperReceived += OnWhisperReceived;
            //Client.OnNewSubscriber += OnNewSubscriber;
            //Client.OnConnected += OnConnected;
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);

            if (Client.IsConnected)
            {
                Client.Disconnect();
            }

            //Client.OnConnected += OnConnected;
            //Client.OnNewSubscriber += OnNewSubscriber;
            //Client.OnWhisperReceived += OnWhisperReceived;
            Client.OnError -= OnError;
            Client.OnMessageReceived -= OnMessageReceived;
            Client.OnMessageSent -= OnMessageSent;
            Client.OnLeftChannel -= OnLeftChannel;
            Client.OnJoinedChannel -= OnJoinedChannel;
            Client.OnLog -= OnLog;

            Client = null!;
        }

        #endregion Init/Deinit
        #region Actions

        public void Connect()
        {
            try
            {
                Client.Initialize(new(Plugin.PluginConfig.Username, Plugin.PluginConfig.OAuthCode), Plugin.PluginConfig.ChannelToSend);
                Client.Connect();
            }
            catch (Exception e)
            {
                OnError(this, new OnErrorEventArgs() { Exception = e });
            }
        }

        public void Disconnect() => Client.Disconnect();

        public void SendMessage(JoinedChannel channel, string args) => Client.SendMessage(channel, args);

        public void JoinChannel(string channel, bool overrideCheck = false) => Client.JoinChannel(channel, overrideCheck);
        public void LeaveChannel(JoinedChannel channel) => Client.LeaveChannel(channel);

        #endregion Actions
        #region Event Handlers

        public void OnError(object? sender, OnErrorEventArgs e)
        {
            Plugin.Logger.Print(e.Exception.Message);
        }

        public void OnLog(object sender, OnLogArgs e)
        {
            if (e.Data.Contains("Login authentication failed"))
            {
                Plugin.Logger.Print("Login authentication failed! Please check your username and OAuth code!");
            }
            if (e.Data.StartsWith("Finished channel joining queue.")) 
            {
                return; 
            }
            if (e.Data.Contains("@msg-id=msg_channel_suspended"))
            {
                Plugin.Logger.Print("Unable to join channel " + Plugin.PluginConfig.ChannelToSend + ", channel is suspended or does not exist! Reverting back to your own channel!");
                Plugin.PluginConfig.ChannelToSend = Client.TwitchUsername;
                Client.JoinChannel(Client.TwitchUsername);
            }
            if (e.Data.Contains("Received: @msg-id=") && e.Data.Contains("NOTICE"))
            {
                string Message = Regex.Match(e.Data, Plugin.PluginConfig.ChannelToSend.ToLower() + " :.*").Value.Replace(Plugin.PluginConfig.ChannelToSend.ToLower() + " :", "");
                Plugin.Logger.Print(Message);
            }
            if (e.Data.StartsWith("Received:")) 
            {
                return; 
            }
            if (e.Data.StartsWith("Writing:")) 
            {
                return; 
            }
            if (e.Data.StartsWith("Connecting to")) 
            {
                return;
            }
            if ((e.Data.StartsWith("Joining ") || e.Data.StartsWith("Leaving "))) 
            {
                return; 
            }
            if (e.Data == "Should be connected!")
            {
                Plugin.Logger.Print("Connected to Twitch chat!");
                return;
            }
            if (e.Data == "Disconnect Twitch Chat Client...")
            {
                Plugin.Logger.Print("Disconnected from Twitch chat!");
                return;
            }
        }

        public void OnJoinedChannel(object sender, OnJoinedChannelArgs e)
        {
            Plugin.Logger.Print($"Joined channel: {e.Channel}");
        }

        public void OnLeftChannel(object sender, OnLeftChannelArgs e)
        {
            Plugin.Logger.Print($"Left channel: {e.Channel}");
        }

        public void OnMessageSent(object sender, OnMessageSentArgs e)
        {
            if (!Plugin.PluginConfig.TwitchEnabled)
            {
                return;
            }
            Plugin.Logger.Print(e.SentMessage);
        }

        public void OnMessageReceived(object sender, OnMessageReceivedArgs e)
        {
            if (!Plugin.PluginConfig.TwitchEnabled) 
            { 
                return; 
            }
            Plugin.Logger.Print(e.ChatMessage);
        }

        #endregion Event Handlers
    }
}
