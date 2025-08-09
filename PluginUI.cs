using Dalamud.Game.ClientState.Party;
using Dalamud.Game.Command;
using Dalamud.Game.Gui;
using Dalamud.Game;
using Dalamud.Plugin;
using Dalamud.Bindings.ImGui;
using ImGuiScene;
using System;
using System.Diagnostics;
using System.Net;
using System.Numerics;
using TwitchLib.Api.Helix;
using TwitchLib.Client.Models;
using Veda;
using static System.Formats.Asn1.AsnWriter;
using System.Collections.Generic;
using Dalamud.Interface.Utility.Raii;

namespace TwitchXIV
{
    public class PluginUI
    {
        public bool IsVisible;
        public bool ShowSupport;
        public bool PopupOpen = false;
        public static readonly string TwitchClientID = "2dx4tp1c75iyh27w0b1qfgfx23mjau";
        public static readonly string TwitchRedirectUri = "https://aida.moe/TwitchXIV/plugin_return.php";
        public static readonly string RequestedScopes = "user:read:chat+user:read:whispers+user:manage:whispers+user:write:chat+chat:edit+chat:read";

        public void Draw()
        {
            if (!IsVisible || !ImGui.Begin("Twitch XIV Config", ref IsVisible, ImGuiWindowFlags.AlwaysAutoResize))
                return;
            ImGui.Text("Enter your twitch username here:");
            ImGui.SetNextItemWidth(310);
            ImGui.InputText("Username", ref Plugin.PluginConfig.Username, 25);
            ImGui.Text("Enter the initial channel name to join here:");
            ImGui.SetNextItemWidth(310);
            ImGui.InputText("Channel", ref Plugin.PluginConfig.ChannelToSend, 25);
            ImGui.Text("The last channel you join will be remembered and\nautomatically joined at plugin start.");
            ImGui.Text("Enter your oauth code here (including the \"oauth:\" part):");
            ImGui.SetNextItemWidth(310);
            ImGui.InputText("OAuth", ref Plugin.PluginConfig.OAuthCode, 36);
            if (ImGui.Button("Save"))
            {
                if (Plugin.PluginConfig.Username == "Your twitch.tv username")
                {
                    Plugin.Chat.Print(Functions.BuildSeString("TwitchXIV", $"Please enter your twitch username in the first input box.",ColorType.Error));
                    return;
                }
                if (Plugin.PluginConfig.ChannelToSend == "Channel to send chat to")
                {
                    Plugin.Chat.Print(Functions.BuildSeString("TwitchXIV", $"Please enter a channel in the second input box.", ColorType.Error));
                    return;
                }
                if (Plugin.PluginConfig.OAuthCode.Length < 36)
                {
                    Plugin.Chat.Print(Functions.BuildSeString("TwitchXIV", $"Please make sure your oauth code is correct and includes the beginning \"oauth:\" part.", ColorType.Error));
                    return;
                }
                Plugin.PluginConfig.Save();
                if (WOLClient.Client != null)
                {
                    if (WOLClient.Client.IsConnected) { WOLClient.Client.Disconnect(); }
                }
                this.IsVisible = false;
                Plugin.Chat.Print(Functions.BuildSeString("Twitch XIV","<c17>DO <c25>NOT <c37>SHARE <c45>YOUR <c48>OAUTH <c52>CODE <c500>WITH <c579>ANYONE!"));
                WOLClient.DoConnect();
            }
            ImGui.SameLine();
            if (ImGui.Checkbox("Relay twitch chat to chatbox", ref Plugin.PluginConfig.TwitchEnabled))
            {
                Plugin.Chat.Print(Functions.BuildSeString("TwitchXIV",$"Toggled twitch chat {(Plugin.PluginConfig.TwitchEnabled ? "on" : "off")}."));
            }
            ImGui.SameLine();
            ImGui.Indent(275);
            if (ImGui.Button("Get OAuth code"))
            {
                string AuthURL = "https://id.twitch.tv/oauth2/authorize?response_type=code&client_id=" + TwitchClientID + "&redirect_uri=" + TwitchRedirectUri + "&scope=" + RequestedScopes;
                Functions.OpenWebsite(AuthURL);
                //Functions.OpenWebsite("https://twitchapps.com/tmi/"); 
            }
            ImGui.Spacing();
            ImGui.Indent(-275);
            if (ImGui.Button("Want to help support my work?"))
            {
                ShowSupport = !ShowSupport;
            }
            if (ImGui.IsItemHovered()) { ImGui.SetTooltip("Click me!"); }
            if (ShowSupport)
            {
                ImGui.Text("Here are the current ways you can support the work I do.\nEvery bit helps, thank you! Have a great day!");
                ImGui.PushStyleColor(ImGuiCol.Button, new System.Numerics.Vector4(0.19f, 0.52f, 0.27f, 1));
                if (ImGui.Button("Donate via Paypal"))
                {
                    Functions.OpenWebsite("https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=QXF8EL4737HWJ");
                }
                ImGui.PopStyleColor();
                ImGui.SameLine();
                ImGui.PushStyleColor(ImGuiCol.Button, new System.Numerics.Vector4(0.95f, 0.39f, 0.32f, 1));
                if (ImGui.Button("Become a Patron"))
                {
                    Functions.OpenWebsite("https://www.patreon.com/bePatron?u=5597973");
                }
                ImGui.PopStyleColor();
                ImGui.SameLine();
                ImGui.PushStyleColor(ImGuiCol.Button, new System.Numerics.Vector4(0.25f, 0.67f, 0.87f, 1));
                if (ImGui.Button("Support me on Ko-Fi"))
                {
                    Functions.OpenWebsite("https://ko-fi.com/Y8Y114PMT");
                }
                ImGui.PopStyleColor();
            }
            ImGui.End();
        }
    }
}
