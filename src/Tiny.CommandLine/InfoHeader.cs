////////////////////////////////////////////////////////////////////////////
// Copyright 2022 Ivan Vinogradov
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
// Version $(Version)
////////////////////////////////////////////////////////////////////////////
//
// Example:
//    using Tiny.CommandLine;
//
//    new CommandLineParser(args, "executable_file_name", "Description of the entire tool")
//        .Command("sub_command", "Description of this sub-command", parser =>
//        {
//            parser
//                // ... other options ...
//                .Run();
//
//            /* sub-command logic */
//        })
//        .Option('o', "option_name", out string option1, "What does this option do", required: true, valueName: "value")
//        .Argument(out string argument1, "Why is this argument needed")
//        .Check(() => argument1 != null, "The argument1 must be not null")
//        .Run();
//
//    MainCommandHandler(option1, argument1);
//
//    static void MainCommandHandler(string option1, string argument1)
//    {
//        /* main command logic */
//    }
//
////////////////////////////////////////////////////////////////////////////