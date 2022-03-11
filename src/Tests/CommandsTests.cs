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
            bool cmdInvoked = false;

            var res = CreateParser(cmd)
                .Command("test", null, b =>
                {
                    var res = b.GetResult();
                    Assert.IsFalse(res);
                })
                .Command("cmd", null, b =>
                {
                    var res = b.GetResult();
                    Assert.IsTrue(res);
                    cmdInvoked = true;
                })
                .GetResult();

            Assert.IsFalse(res);
            Assert.IsTrue(cmdInvoked);
        }

        [TestCase("--flag cmd")]
        public void Options_defined_before_command_should_be_parsed_first(string cmd)
        {
            bool invokedFlag = false;
            var res = CreateParser(cmd)
                .Option("flag", out bool flag)
                .Command("cmd", null, b =>
                {
                    var res = b.GetResult();
                    Assert.IsTrue(res);

                    invokedFlag = flag;
                })
                .GetResult();

            Assert.IsFalse(res);
            Assert.IsTrue(invokedFlag);
        }

        [TestCase("cmd sub")]
        public void Specified_sub_commands_should_be_invoked(string cmd)
        {
            bool cmdInvoked = false;

            var res = CreateParser(cmd)
                .Command("cmd", null, b =>
                {
                    var res = b
                        .Command("sub", null, s =>
                        {
                            var res = s.GetResult();
                            Assert.IsTrue(res);
                            cmdInvoked = true;
                        })
                        .GetResult();

                    Assert.IsFalse(res);
                })
                .GetResult();

            Assert.IsFalse(res);
            Assert.IsTrue(cmdInvoked);
        }

        [TestCase("--test cmd -f")]
        public void Only_options_declared_for_current_command_should_be_parsed(string cmd)
        {
            bool cmdInvoked = false;

            var res = CreateParser(cmd)
                .Option("test", out bool _)
                .Command("cmd", null, b =>
                {
                    var res = b
                        .Option('f', out bool innerFlag)
                        .GetResult();

                    Assert.IsTrue(res);
                    Assert.IsTrue(innerFlag);
                    cmdInvoked = true;
                })
                .Option('f', out bool outerFlag)
                .GetResult();

            Assert.IsFalse(res);
            Assert.IsTrue(cmdInvoked);
            Assert.IsFalse(outerFlag);
        }
    }
}