using NUnit.Framework;
// ReSharper disable AccessToModifiedClosure

namespace System.TinyCommandLine.Tests
{
    [TestFixture]
    public class CommandsTests
    {
        [TestCase("cmd")]
        [TestCase("--flag cmd")]
        public void Only_specified_command_should_be_invoked(string cmd)
        {
            bool invokedTest = false;
            bool invokedCmd = false;
            bool invokedBase = false;

            Run(cmd, s => s
                .Command("test", b => b
                    .Handler(() => invokedTest = true)
                )
                .Command("cmd", b => b
                    .Handler(() => invokedCmd = true)
                )
                .Handler(() => invokedBase = true)
            );

            Assert.IsTrue(invokedCmd);
            Assert.IsFalse(invokedBase);
            Assert.IsFalse(invokedTest);
        }

        [TestCase("--flag cmd")]
        public void Command_should_be_invoked_after_options(string cmd)
        {
            bool invokedFlag = false;
            Run(cmd, s => s
                .Variable(out bool flag)
                .Command("cmd", b => b
                    .Handler(() => invokedFlag = flag)
                )
                .Option("flag", out flag)
            );

            Assert.IsTrue(invokedFlag);
        }

        [TestCase("cmd sub")]
        public void Specified_SubCommand_should_be_invoked(string cmd)
        {
            bool invokedCmd = false;
            bool invokedSub = false;
            bool invokedBase = false;

            Run(cmd, s => s
                .Command("cmd", b => b
                    .Command("sub", bs => bs
                        .Handler(() => invokedSub = true)
                    )
                    .Handler(() => invokedCmd = true)
                )
                .Handler(() => invokedBase = true)
            );

            Assert.IsFalse(invokedBase);
            Assert.IsFalse(invokedCmd);
            Assert.IsTrue(invokedSub);
        }

        [TestCase("", ExpectedResult = "...")]
        [TestCase("-v", ExpectedResult = "v..")]
        [TestCase("-v cmd", ExpectedResult = "v..")]
        [TestCase("-v cmd sub", ExpectedResult = "v..")]
        [TestCase("cmd -v", ExpectedResult = ".v.")]
        [TestCase("cmd -v sub", ExpectedResult = ".v.")]
        [TestCase("cmd sub -v", ExpectedResult = "..v")]
        [TestCase("-v cmd sub -v", ExpectedResult = "v.v")]
        [TestCase("cmd -v sub -v", ExpectedResult = ".vv")]
        [TestCase("-v cmd -v sub -v", ExpectedResult = "vvv")]
        public string Option_in_root_in_command_and_in_sub_command_can_have_same_names(string cmd)
        {
            var result = string.Empty;

            Run(cmd, s => s
                .Variable(out bool rootFlag)
                .Command("cmd", b => b
                    .Variable(out bool cmdFlag)
                    .Command("sub", bs => bs
                        .Option('v', "value", out bool subFlag)
                        .Handler(() => result = Sum(rootFlag, cmdFlag, subFlag))
                    )
                    .Option('v', "value", out cmdFlag)
                    .Handler(() => result = Sum(rootFlag, cmdFlag))
                )
                .Option('v', "value", out rootFlag)
                .Handler(() => result = Sum(rootFlag))
            );

            return result;

            static string Sum(bool f1, bool f2 = false, bool f3 = false) => (f1 ? "v" : ".") + (f2 ? "v" : ".") + (f3 ? "v" : ".");
        }


        [TestCase("--test cmd -f")]
        public void Only_options_declared_for_current_command_should_be_parsed(string cmd)
        {
            bool innerFlag = false;
            bool outerFlag = false;

            Run(cmd, s => s
                .Command("cmd", b => b
                    .Option('f', out innerFlag)
                    .Handler(() => { })
                )
                .Option('f', out outerFlag)
                .Option("test", out bool _)
            );

            Assert.IsTrue(innerFlag);
            Assert.IsFalse(outerFlag);
        }

        static void Run(string cmd, Implementation.CommandConfigurator configure)
        {
            var args = Helper.SplitArguments(cmd);
            CommandLineParser.Run(args, configure);
        }
    }
}