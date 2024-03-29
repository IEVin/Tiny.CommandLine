using NUnit.Framework;
using static Tiny.CommandLine.Tests.Helper;

namespace Tiny.CommandLine.Tests
{
    [TestFixture]
    public class CommandsTests
    {
        [OneTimeSetUp]
        protected void SetUp()
        {
            OverrideOutput();
            OverrideExit();
        }

        [TestCase("cmd")]
        public void Only_specified_command_should_be_invoked(string cmd)
        {
            bool cmdInvoked = false;

            var res = CreateParser(cmd)
                .Command("test", null, b =>
                {
                    var res1 = b.GetResult();
                    Assert.IsFalse(res1);
                })
                .Command("cmd", null, b =>
                {
                    var res1 = b.GetResult();
                    Assert.IsTrue(res1);
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
                    var res1 = b.GetResult();
                    Assert.IsTrue(res1);

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
                    var res1 = b
                        .Command("sub", null, s =>
                        {
                            var res2 = s.GetResult();
                            Assert.IsTrue(res2);
                            cmdInvoked = true;
                        })
                        .GetResult();

                    Assert.IsFalse(res1);
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
                    var res1 = b
                        .Option('f', out bool innerFlag)
                        .GetResult();

                    Assert.IsTrue(res1);
                    Assert.IsTrue(innerFlag);
                    cmdInvoked = true;
                })
                .Option('f', out bool outerFlag)
                .GetResult();

            Assert.IsFalse(res);
            Assert.IsTrue(cmdInvoked);
            Assert.IsFalse(outerFlag);
        }

        [TestCase("cmd")]
        public void Command_should_not_parsed_if_has_error(string cmd)
        {
            var res = CreateParser(cmd)
                .Option('f', out bool _, required: true)
                .Command("cmd", null, _ => Assert.Fail())
                .GetResult();

            Assert.AreEqual(res.Result, ParserResult.State.Error);
        }

        [TestCase("cmd")]
        public void Run_should_exit_when_sub_command_invoked(string cmd)
        {
            var parser = CreateParser(cmd)
                .Command("cmd", null, p => p.GetResult());

            var ex = Assert.Catch<ExitException>(() => parser.Run());
            Assert.NotNull(ex);
            Assert.AreEqual(ex.Code, 0);
        }
    }
}