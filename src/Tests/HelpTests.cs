﻿using NUnit.Framework;
using static System.TinyCommandLine.Tests.Helper;

namespace System.TinyCommandLine.Tests
{
    [TestFixture]
    public class HelpTests
    {
        [TestCase("-h")]
        [TestCase("--help")]
        public void Help_option_should_be_parsed(string cmd)
        {
            Run(cmd, s => s
                .Option('v', "value", out int _)
            );
        }

        [TestCase("-h")]
        public void Help_with_required_options_should_be_invoked(string cmd)
        {
            Run(cmd, s => s
                .Option('v', "value", out int _, b => b
                    .Required()
                )
            );
        }
    }
}