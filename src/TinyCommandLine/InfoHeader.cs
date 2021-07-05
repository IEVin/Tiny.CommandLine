////////////////////////////////////////////////////////////////////////////
// Copyright 2021 Ivan Vinogradov
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
////////////////////////////////////////////////////////////////////////////
//
// Example:
//    using Tiny.CommandLine;
//
//    CommandLineParser.Run("executable_file_name", args, c => c
//        .HelpText("Description of the entire tool")
//        .Command("sub_command", c1 => c1
//            .HelpText("Description of this subcommand")
//            // ...
//            .Handler(() => { /* sub_command handler definition */ })
//        )
//        .Option('o', "option_name", out string option1, s => s
//            .Required()
//            .HelpText("What does this option do")
//            .ValueName("value")
//        )
//        .Argument(out string argument1, "Why is this argument needed")
//        .Check(() => argument1 != null, "The argument1 must be not null")
//        .Handler(() => CommandHandler(option1, argument1))
//    );
//
//    static void CommandHandler(string option1, string argument1)
//    {
//        // ...
//    }
//
////////////////////////////////////////////////////////////////////////////