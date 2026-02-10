using Dalamud.Bindings.ImGui;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Numerics;
using TwitchXIV.Chat;

namespace TwitchXIV
{
    public sealed class ChatUi
    {
        #region Variables

        private readonly ConcurrentQueue<(string channel, string user, string message, Vector4 userColor)> PendingLines = new();
        private readonly List<WrappedChatLine> ChatLines = [];
        private readonly ChatRenderer ChatRenderer = new();

        private static readonly Vector2 InitialSize = new(520, 260);

        private bool IsVisible = true;
        private bool Clear = false;

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
            UpdateLines();

            ImGui.PushStyleColor(ImGuiCol.WindowBg, Plugin.PluginConfig.BackgroundColor);

            ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 12f);
            ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 0f);
            ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(12, 10));
            ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(4, 2));
            ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(6, 6));

            ImGui.SetNextWindowSize(ChatUi.InitialSize, ImGuiCond.FirstUseEver);
            if (!IsVisible || !ImGui.Begin("#twitch", ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoScrollbar))
            {
                ImGui.End();
                ImGui.PopStyleColor();
                ImGui.PopStyleVar(5);
                return;
            }

            ChatRenderer.DrawLines(ChatLines);

            ImGui.End();
            ImGui.PopStyleColor();
            ImGui.PopStyleVar(5);
        }

        #endregion Drawing
        #region Line Updates

        public void ClearLines()
        {
            Clear = true;
        }

        public void AddLine(string channel, string user, string message, Vector4 userColor)
        {
            PendingLines.Enqueue((channel, user, message, userColor));
        }

        public void PruneLines(int limit)
        {
            if (Clear)
            {
                Clear = false;
                ChatLines.Clear();
                return;
            }

            while (ChatLines.Count > limit)
            {
                ChatLines.RemoveAt(0);
            }
        }

        private void UpdateLines()
        {
            while (PendingLines.TryDequeue(out var line))
            {
                ChatLines.Add(new($"[{DateTime.Now:HH:mm}]", $"[{line.channel}]", line.user, line.message, line.userColor));
            }
            PruneLines(Plugin.PluginConfig.MaxChatlines);
        }

        #endregion Line Updates
    }
}
