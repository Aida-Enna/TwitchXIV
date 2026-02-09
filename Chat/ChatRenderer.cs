using Dalamud.Bindings.ImGui;
using System.Collections.Generic;
using System.Numerics;

namespace TwitchXIV.Chat
{
    public sealed class ChatRenderer
    {
        #region Drawing Lines
        public void DrawLines(List<WrappedChatLine> lines)
        {
            if (!ImGui.BeginChild("#chat", Vector2.Zero, false, ImGuiWindowFlags.AlwaysVerticalScrollbar))
            {
                ImGui.EndChild();
                return;
            }

            foreach (var line in lines)
            {
                DrawLine(line);
            }

            if (ImGui.GetScrollY() >= ImGui.GetScrollMaxY())
            {
                ImGui.SetScrollHereY(1f);
            }

            ImGui.EndChild();
        }

        private void DrawLine(WrappedChatLine line)
        {
            var drawList = ImGui.GetWindowDrawList();
            var start = ImGui.GetCursorPosX();

            // timestamp
            ImGui.TextColored(Plugin.PluginConfig.TimeColor, line.Timestamp);
            ImGui.SameLine(0, 6);

            // channel
            ImGui.TextColored(Plugin.PluginConfig.ChannelColor, line.Channel);
            ImGui.SameLine(0, 6);

            // username
            ImGui.TextColored(line.UserColor, $"{line.User}:");
            ImGui.SameLine(0, 6);

            // message
            ChatCache.WrapLine(line, ImGui.GetFontSize(), ImGui.GetContentRegionAvail().X);
            ImGui.TextColored(Plugin.PluginConfig.MessageColor, line.WrappedText);

            // highlight
            DrawHighlight(line, start, drawList);
        }

        #endregion Drawing Lines
        #region Drawing Highlight

        private void DrawHighlight(ChatLine line, float startX, ImDrawListPtr drawList)
        {
            var min = new Vector2(ImGui.GetWindowPos().X + startX, ImGui.GetItemRectMin().Y);
            var max = ImGui.GetItemRectMax();
            var hovered = ImGui.IsMouseHoveringRect(min, max);

            if (hovered)
            {
                drawList.AddRectFilled(min, max, ImGui.GetColorU32(Plugin.PluginConfig.HighlightColor), 4f);

                ImGui.SetTooltip("click to copy");

                if (ImGui.IsMouseClicked(ImGuiMouseButton.Left))
                {
                    ImGui.SetClipboardText(line.FullText);
                }
            }
        }

        #endregion Drawing Highlight
    }
}
