using NUnit.Framework;
using static Tiny.CommandLine.Tests.Helper;

namespace Tiny.CommandLine.Tests
{
    [TestFixture]
    public class CommandsTests
    {
        [TestCase("cmd")]
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
        public void Options_defined_before_command_should_be_parsed_first(string cmd)
        {
            bool invokedFlag = false;
            Run(cmd, s => s
                .Option("flag", out bool flag)
                .Command("cmd", b => b
                    .Handler(() => invokedFlag = flag)
                )
            );

            Assert.IsTrue(invokedFlag);
        }

        [TestCase("cmd sub")]
        public void Specified_sub_commands_should_be_invoked(string cmd)
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

        [TestCase("--test cmd -f")]
        public void Only_options_declared_for_current_command_should_be_parsed(string cmd)
        {
            bool innerFlag = false;
            bool outerFlag = false;

            Run(cmd, s => s
                .Option("test", out bool _)
                .Command("cmd", b => b
                    .Option('f', out innerFlag)
                    .Handler(() => { })
                )
                .Option('f', out outerFlag)
            );

            Assert.IsTrue(innerFlag);
            Assert.IsFalse(outerFlag);
        }
    }
}