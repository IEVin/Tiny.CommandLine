using System.Collections.Generic;
using NUnit.Framework;

namespace System.TinyCommandLine.Tests
{
    [TestFixture]
    public class OptionsTests
    {
        [TestCase("-v=val")]
        [TestCase("--value=val")]
        public void Value_can_be_separated_by_equal(string cmd) => Assert.AreEqual("val", ParseOption<string>(cmd));

        [TestCase("-v val")]
        [TestCase("--value val")]
        public void Value_can_be_separated_by_space(string cmd) => Assert.AreEqual("val", ParseOption<string>(cmd));

        [TestCase("-v \"value with space\"")]
        public void Value_in_quote_should_be_parsed_correctly(string cmd) => Assert.AreEqual("value with space", ParseOption<string>(cmd));

        [TestCase("-v --value", ExpectedResult = "--value")]
        [TestCase("--value -v", ExpectedResult = "-v")]
        [TestCase("--value \"--value=test\"", ExpectedResult = "--value=test")]
        [TestCase("--value \"-v\"", ExpectedResult = "-v")]
        public string Values_that_look_like_a_flag_should_be_parsed_correctly(string cmd) => ParseOption<string>(cmd);

        [TestCase("-v 10", ExpectedResult = 10)]
        [TestCase("--value -7", ExpectedResult = -7)]
        public int Int_should_be_parsed_correctly(string cmd) => ParseOption<int>(cmd);

        [TestCase("-v -1.5", ExpectedResult = -1.5)]
        [TestCase("--value 0.8", ExpectedResult = 0.8)]
        public double Double_should_be_parsed_correctly(string cmd) => ParseOption<double>(cmd);

        [TestCase("-v")]
        [TestCase("--value")]
        public void Flag_should_be_parsed_as_bool(string cmd) => Assert.IsTrue(ParseOption<bool>(cmd));

        [TestCase("-v=1")]
        [TestCase("-v=true")]
        [TestCase("-v=True")]
        [TestCase("--value=1")]
        [TestCase("--value=true")]
        [TestCase("--value=True")]
        public void True_should_be_parsed_as_bool(string cmd) => Assert.IsTrue(ParseOption<bool>(cmd));

        [TestCase("-v=0")]
        [TestCase("-v=false")]
        [TestCase("-v=False")]
        [TestCase("--value=0")]
        [TestCase("--value=false")]
        [TestCase("--value=False")]
        public void False_should_be_parsed_as_bool(string cmd)
        {
            var result = true;
            Run(cmd, s => s.Option('v', "value", out result, b => b.Default(true)));

            Assert.IsFalse(result);
        }

        [TestCase("-v 1 --value test -v qq")]
        public void IReadOnlyList_should_be_parsed_correctly(string cmd)
        {
            IReadOnlyList<string> list = null;
            Run(cmd, s => s.OptionList('v', "value", out list));

            Assert.NotNull(list);
            Assert.AreEqual(list.Count, 3);
            Assert.AreEqual(list[0], "1");
            Assert.AreEqual(list[1], "test");
            Assert.AreEqual(list[2], "qq");
        }

        static T ParseOption<T>(string cmd)
        {
            T result = default;
            Run(cmd, s => s.Option('v', "value", out result));

            return result;
        }

        static void Run(string cmd, Implementation.CommandConfigurator configure)
        {
            var args = Helper.SplitArguments(cmd);
            CommandLineParser.Run(args, configure);
        }
    }
}
