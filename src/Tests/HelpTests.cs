using NUnit.Framework;
using static Tiny.CommandLine.Tests.Helper;

namespace Tiny.CommandLine.Tests
{
    [TestFixture]
    public class HelpTests
    {
        [OneTimeSetUp]
        protected void SetUp()
        {
            OverrideOutput();
            OverrideExit();
        }

        [TestCase("-h")]
        [TestCase("--help")]
        public void Help_option_should_be_parsed(string cmd)
        {
            var res = CreateParser(cmd)
                .Option('v', "value", out int _)
                .GetResult();

            CheckHelpInvoked(res);
        }

        [TestCase("-h")]
        [TestCase("--help")]
        public void Help_after_argument_should_be_invoked(string cmd)
        {
            var res = CreateParser(cmd)
                .Argument(out string _)
                .GetResult();

            CheckHelpInvoked(res);
        }

        [TestCase("-h")]
        public void Help_with_required_options_should_be_invoked(string cmd)
        {
            var res = CreateParser(cmd)
                .Option('v', "value", out int _, required: true)
                .GetResult();

            CheckHelpInvoked(res);
        }

        [TestCase("--help")]
        public void Run_should_exit_when_help_require(string cmd)
        {
            var parser = CreateParser(cmd)
                .Option('v', out int _);

            var ex = Assert.Catch<ExitException>(() => parser.Run());
            Assert.NotNull(ex);
            Assert.AreEqual(ex.Code, 0);
        }

        [TestCase("cmd -h")]
        public void Help_should_be_invoked_only_for_actual_command(string cmd)
        {
            var res = CreateParser(cmd)
                .Command("cmd", "command 1", p =>
                {
                    var res2 = p
                        .Command("cmd2", "command 2", _ => Assert.Fail())
                        .Option('f', out bool _, "force")
                        .GetResult();

                    CheckHelpInvoked(res2);
                })
                .Option('v', "value", out int _, required: true)
                .GetResult();

            Assert.AreEqual(res.Result, ParserResult.State.Handled);
        }

        public static void CheckHelpInvoked(ParserResult result) => Assert.AreEqual(result.Result, ParserResult.State.HelpRequired);
    }
}