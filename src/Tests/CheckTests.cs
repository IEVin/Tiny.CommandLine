using NUnit.Framework;
using static Tiny.CommandLine.Tests.Helper;

namespace Tiny.CommandLine.Tests
{
    [TestFixture]
    public class CheckTests
    {
        [OneTimeSetUp]
        protected void SetUp()
        {
            OverrideOutput();
            OverrideExit();
        }

        [TestCase("-v 1")]
        public void Check_should_be_invoked(string cmd)
        {
            bool check = false;

            var res = CreateParser(cmd)
                .Option('v', out int _)
                .Check(() => check = true, "")
                .GetResult();

            Assert.AreEqual(res.Result, ParserResult.State.Success);
            Assert.IsTrue(check);
        }

        [TestCase("cmd1", ExpectedResult = 1)]
        [TestCase("cmd2", ExpectedResult = 2)]
        [TestCase("", ExpectedResult = 10)]
        public int Only_check_in_actual_command_should_be_invoked(string cmd)
        {
            int check = 0;

            var res = CreateParser(cmd)
                .Command("cmd1", null, p => p.Check(() => (check += 1) > 0, ""))
                .Command("cmd2", null, p => p.Check(() => (check += 2) > 0, ""))
                .Check(() => (check += 10) > 0, "")
                .GetResult();

            Assert.IsTrue(res.Result == ParserResult.State.Success || res.Result == ParserResult.State.Handled);
            return check;
        }

        [TestCase("")]
        public void Failed_check_should_fail_parsing(string cmd)
        {
            var res = CreateParser(cmd)
                .Check(() => false, "")
                .GetResult();

            Assert.AreEqual(res.Result, ParserResult.State.Error);
        }

        [TestCase("-v aaa")]
        public void Check_should_be_skipped_when_parsing_failed(string cmd)
        {
            bool check = false;

            var res = CreateParser(cmd)
                .Option('v', out int _)
                .Check(() => check = true, "")
                .GetResult();

            Assert.AreEqual(res.Result, ParserResult.State.Error);
            Assert.IsFalse(check);
        }

        [TestCase("")]
        public void Check_should_be_skipped_when_required_option_missing(string cmd)
        {
            bool check = false;

            var res = CreateParser(cmd)
                .Option('v', out int _, required: true)
                .Check(() => check = true, "")
                .GetResult();

            Assert.AreEqual(res.Result, ParserResult.State.Error);
            Assert.IsFalse(check);
        }

        [TestCase("--help")]
        public void Check_should_be_skipped_when_help_required(string cmd)
        {
            bool check = false;

            var res = CreateParser(cmd)
                .Check(() => check = true, "")
                .GetResult();

            Assert.AreEqual(res.Result, ParserResult.State.HelpRequired);
            Assert.IsFalse(check);
        }
    }
}