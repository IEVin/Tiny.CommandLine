using System;

namespace Tiny.CommandLine.Implementation
{
    class CommandState
    {
        public string SubCommandName;
        public CommandConfigurator SubCommandHandler;
        public Action Handler;
        public string ErrReason;
        public bool IsFinished;
        public bool IsHelpRequired;
        public bool IsHelpChecked;
    }
}