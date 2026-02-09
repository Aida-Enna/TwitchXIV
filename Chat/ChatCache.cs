using Dalamud.Bindings.ImGui;
using System;
using System.Collections.Generic;
using System.Text;

namespace TwitchXIV.Chat
{
    public static class ChatCache
    {
        #region Variables

        private static readonly Dictionary<char, float> GlyphWidthCache = [];
        private static float CachedFontScale = -1f;

        #endregion Variables
        #region Caching

        public static float GetWidth(char c)
        {
            var fontScale = ImGui.GetFontSize();

            if (!MathF.Abs(fontScale - ChatCache.CachedFontScale).Equals(0))
            {
                ChatCache.GlyphWidthCache.Clear();
                ChatCache.CachedFontScale = fontScale;
            }

            if (!ChatCache.GlyphWidthCache.TryGetValue(c, out float width))
            {
                width = ImGui.CalcTextSize(c.ToString()).X;
                ChatCache.GlyphWidthCache[c] = width;
            }

            return width;
        }

        public static float GetWidth(string text)
        {
            var width = 0f;
            foreach (char c in text)
            {
                width += ChatCache.GetWidth(c);
            }
            return width;
        }

        #endregion Caching
        #region Wrapping

        public static void WrapLine(WrappedChatLine line, float fontScale, float maxWidth)
        {
            if (MathF.Abs(line.WrappedLength - maxWidth) < 0.5f && MathF.Abs(fontScale - ChatCache.CachedFontScale) < 0.01f)
            {
                return;
            }

            line.WrappedText = ChatCache.WrapText(line.Message, maxWidth);
            line.WrappedLength = maxWidth;
        }

        private static string WrapText(string text, float maxWidth)
        {
            // thanks ChatGPT. i'm too lazy to do this myself
            var builder = new StringBuilder(text.Length);
            var lineWidth = 0f;

            int i = 0;
            while (i < text.Length)
            {
                if (text[i] == '\n')
                {
                    builder.Append('\n');
                    lineWidth = 0f;
                    ++i;
                    continue;
                }

                int nextSpace = text.IndexOf(' ', i);
                if (nextSpace == -1)
                {
                    nextSpace = text.Length;
                }

                var word = text[i..nextSpace];
                var wordWidth = ChatCache.GetWidth(word);

                if (lineWidth + wordWidth <= maxWidth)
                {
                    builder.Append(word);
                    lineWidth += wordWidth;
                }
                else
                {
                    foreach (char c in word)
                    {
                        float charWidth = ChatCache.GetWidth(c);

                        if (lineWidth + charWidth > maxWidth && lineWidth > 0)
                        {
                            builder.Append('\n');
                            lineWidth = 0f;
                        }

                        builder.Append(c);
                        lineWidth += charWidth;
                    }
                }

                if (nextSpace < text.Length)
                {
                    float spaceWidth = ChatCache.GetWidth(' ');
                    if (lineWidth + spaceWidth > maxWidth)
                    {
                        builder.Append('\n');
                        lineWidth = 0f;
                    }
                    else
                    {
                        builder.Append(' ');
                        lineWidth += spaceWidth;
                    }
                }

                i = nextSpace + 1;
            }

            return builder.ToString();
        }
    }

    #endregion Wrapping
}
