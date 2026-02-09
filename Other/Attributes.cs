using System;

namespace TwitchXIV.Other
{
    [AttributeUsage(AttributeTargets.Method)]
    public class AliasesAttribute(params string[] aliases) : Attribute
    {
        public string[] Aliases { get; } = aliases;
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class CommandAttribute(string command) : Attribute
    {
        public string Command { get; } = command;
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class DoNotShowInHelpAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class HelpMessageAttribute(string helpMessage) : Attribute
    {
        public string HelpMessage { get; } = helpMessage;
    }
}
