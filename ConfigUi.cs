using Dalamud.Bindings.ImGui;
using TwitchXIV.Other;

namespace TwitchXIV
{
    public class ConfigUi
    {
        #region Variables

        public static readonly string TwitchClientID = "2dx4tp1c75iyh27w0b1qfgfx23mjau";
        public static readonly string TwitchRedirectUri = "https://aida.moe/TwitchXIV/plugin_return.php";
        public static readonly string RequestedScopes = "user:read:chat+user:read:whispers+user:manage:whispers+user:write:chat+chat:edit+chat:read";

        private bool IsVisible = false;
        private bool ShowSupport = false;

        #endregion Variables
        #region Visibility

        public void ToggleVisibility()
        {
            IsVisible = !IsVisible;
        }

        #endregion Visibility
        #region Drawing

        public void Draw()
        {
            ImGui.SetNextWindowSize(new(400, 0), ImGuiCond.Always);
            if (!IsVisible || !ImGui.Begin("Twitch XIV Config", ref IsVisible, ImGuiWindowFlags.AlwaysAutoResize))
            {
                ImGui.End();
                return;
            }

            var regionWidth = ImGui.GetContentRegionAvail().X;

            // enable
            if (ImGui.Checkbox("Enable", ref Plugin.PluginConfig.TwitchEnabled))
            {
                Plugin.Logger.Print($"Toggled twitch chat {(Plugin.PluginConfig.TwitchEnabled ? "on" : "off")}.");
            }
            ImGui.Spacing();

            // username
            ImGui.Text("Username");
            ImGui.SetNextItemWidth(regionWidth);
            ImGui.InputText("##username", ref Plugin.PluginConfig.Username, 25);
            ImGui.Spacing();

            // twitch channel
            ImGui.Text("Twitch Channel");
            ImGui.SetNextItemWidth(regionWidth);
            ImGui.InputText("##tchannel", ref Plugin.PluginConfig.ChannelToSend, 25);
            ImGui.Spacing();

            // oauth
            ImGui.Text("OAuth");
            ImGui.SetNextItemWidth(regionWidth);
            ImGui.InputText("##oauth", ref Plugin.PluginConfig.OAuthCode, 36, ImGuiInputTextFlags.Password);

            // get oauth button
            ImGui.SetNextItemWidth(regionWidth);
            if (ImGui.Button("Get OAuth", new(regionWidth, 0)))
            {
                string AuthURL = "https://id.twitch.tv/oauth2/authorize?response_type=code&client_id=" + TwitchClientID + "&redirect_uri=" + TwitchRedirectUri + "&scope=" + RequestedScopes;
                Functions.OpenWebsite(AuthURL);
            }
            ImGui.Spacing();
            ImGui.Spacing();
            ImGui.Spacing();

            // reconnect
            if (ImGui.Button("Reconnect", new(regionWidth, 0)))
            {
                if (Plugin.PluginConfig.Username == "Your twitch.tv username")
                {
                    //Functions.Print($"Please enter your twitch username in the first input box.", ColorType.Error); TODO
                    return;
                }
                if (Plugin.PluginConfig.ChannelToSend == "Channel to send chat to")
                {
                    //Functions.Print($"Please enter a channel in the second input box.", ColorType.Error); TODO
                    return;
                }
                if (Plugin.PluginConfig.OAuthCode.Length < 36)
                {
                    //Functions.Print($"Please make sure your oauth code is correct and includes the beginning \"oauth:\" part.", ColorType.Error); TODO
                    return;
                }

                if (Plugin.TwitchClient.IsConnected)
                {
                    Plugin.TwitchClient.Disconnect();
                }
                Plugin.TwitchClient.Dispose();

                //Functions.Print("<c17>DO <c25>NOT <c37>SHARE <c45>YOUR <c48>OAUTH <c52>CODE <c500>WITH <c579>ANYONE!"); TODO
                Plugin.TwitchClient = new();
                Plugin.TwitchClient.Connect();
            }
            ImGui.Spacing();
            ImGui.Spacing();
            ImGui.Spacing();

            // background color for custom window
            if (ImGui.CollapsingHeader("Background Color"))
            {
                ImGui.ColorPicker4("##colorBackground", ref Plugin.PluginConfig.BackgroundColor);
            }

            // timestamp color for custom window
            if (ImGui.CollapsingHeader("Timestamp Color"))
            {
                ImGui.ColorPicker4("##timestampColor", ref Plugin.PluginConfig.TimeColor);
            }

            // channel color for custom window
            if (ImGui.CollapsingHeader("Channel Color"))
            {
                ImGui.ColorPicker4("##channelColor", ref Plugin.PluginConfig.ChannelColor);
            }

            // message color for custom window
            if (ImGui.CollapsingHeader("Message Color"))
            {
                ImGui.ColorPicker4("##messageColor", ref Plugin.PluginConfig.MessageColor);
            }
            ImGui.Spacing();

            // show support
            if (ImGui.Button("Want to help support the original author?", new(regionWidth, 0)))
            {
                ShowSupport = !ShowSupport;
            }
            if (ImGui.IsItemHovered())
            {
                ImGui.SetTooltip("Click me!");
            }

            // support
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

        #endregion Drawing
    }
}
