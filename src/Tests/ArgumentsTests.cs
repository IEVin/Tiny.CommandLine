using System.Collections.Generic;
using NUnit.Framework;
using static Tiny.CommandLine.Tests.Helper;

namespace Tiny.CommandLine.Tests
{
    [TestFixture]
    public class ArgumentsTests
    {
        [OneTimeSetUp]
        protected void SetUp()
        {
            OverrideOutput();
            OverrideExit();
        }

        [TestCase("test2", ExpectedResult = "test2|")]
        [TestCase("arg1 arg2", ExpectedResult = "arg1|arg2")]
        public string Arguments_should_be_parsed_correctly(string cmd)
        {
            CreateParser(cmd)
                .Argument(out string arg1)
                .Argument(out string arg2)
                .Run();

            return arg1 + '|' + arg2;
        }

        [TestCase("a b c 23a F_", ExpectedResult = 5)]
        [TestCase("a b c", ExpectedResult = 3)]
        public int Arguments_list_should_be_parsed_correctly(string cmd)
        {
            CreateParser(cmd)
                .ArgumentList(out IReadOnlyList<string> list)
                .Run();

            return list?.Count ?? 0;
        }

        [TestCase("a1 b2 c3 _1) -91")]
        public void Arguments_should_parse_before_arguments_list(string cmd)
        {
            CreateParser(cmd)
                .Argument(out string arg1)
                .Argument(out string arg2)
                .ArgumentList(out IReadOnlyList<string> list)
                .Run();

            Assert.AreEqual(arg1, "a1");
            Assert.AreEqual(arg2, "b2");

            Assert.NotNull(list);
            Assert.AreEqual(list.Count, 3);
            Assert.AreEqual(list[0], "c3");
            Assert.AreEqual(list[1], "_1)");
            Assert.AreEqual(list[2], "-91");
        }

        [TestCase("", ExpectedResult = ParserResult.State.Success)]
        [TestCase("-v", ExpectedResult = ParserResult.State.Success)]
        [TestCase("-v=aaa", ExpectedResult = ParserResult.State.Error)]
        [TestCase("-h", ExpectedResult = ParserResult.State.HelpRequired)]
        public ParserResult.State Argument_list_default_should_be_empty_list(string cmd)
        {
            var res = CreateParser(cmd)
                .Option('v', out bool _)
                .ArgumentList<string>(out var list)
                .GetResult();

            Assert.NotNull(list);
            Assert.IsTrue(list.Count == 0);
            return res;
        }

        [TestCase("aaa")]
        public void Argument_invalid_value_should_return_error(string cmd)
        {
            var res = CreateParser(cmd)
                .Argument(out int value)
                .GetResult();

            Assert.AreEqual(res.Result, ParserResult.State.Error);
            Assert.AreEqual(value, default(int));
        }

        [TestCase("bbb")]
        public void Argument_list_invalid_value_should_return_error(string cmd)
        {
            var res = CreateParser(cmd)
                .ArgumentList<int>(out var list)
                .GetResult();

            Assert.AreEqual(res.Result, ParserResult.State.Error);
            Assert.NotNull(list);
            Assert.IsTrue(list.Count == 0);
        }

        [TestCase("")]
        [TestCase("-f")]
        public void Required_argument_should_return_error_if_missing(string cmd)
        {
            var res = CreateParser(cmd)
                .Option('f', out bool _)
                .Argument(out int _, required: true)
                .GetResult();

            Assert.AreEqual(res.Result, ParserResult.State.Error);
        }

        [TestCase("1 2")]
        [TestCase("1 2 3")]
        public void Not_parsed_token_should_return_error(string cmd)
        {
            var res = CreateParser(cmd)
                .Argument(out int _)
                .GetResult();

            Assert.AreEqual(res.Result, ParserResult.State.Error);
        }
    }
}