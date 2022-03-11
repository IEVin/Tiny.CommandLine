using NUnit.Framework;
using static Tiny.CommandLine.Tests.Helper;

namespace Tiny.CommandLine.Tests
{
    [TestFixture]
    public class HelpTests
    {
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

        public static void CheckHelpInvoked(ParserResult result) => Assert.AreEqual(result.Result, ParserResult.State.HelpRequired);
    }
}