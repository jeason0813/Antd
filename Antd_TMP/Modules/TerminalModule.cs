﻿using System;
using System.IO;
using antdlib.Terminal;
using Nancy;
using Nancy.Security;
//-------------------------------------------------------------------------------------
//     Copyright (c) 2014, Anthilla S.r.l. (http://www..com)
//     All rights reserved.
//
//     Redistribution and use in source and binary forms, with or without
//     modification, are permitted provided that the following conditions are met:
//         * Redistributions of source code must retain the above copyright
//           notice, this list of conditions and the following disclaimer.
//         * Redistributions in binary form must reproduce the above copyright
//           notice, this list of conditions and the following disclaimer in the
//           documentation and/or other materials provided with the distribution.
//         * Neither the name of the Anthilla S.r.l. nor the
//           names of its contributors may be used to endorse or promote products
//           derived from this software without specific prior written permission.
//
//     THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
//     ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
//     WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
//     DISCLAIMED. IN NO EVENT SHALL ANTHILLA S.R.L. BE LIABLE FOR ANY
//     DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
//     (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
//     LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
//     ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
//     (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
//     SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
//
//     20141110
//-------------------------------------------------------------------------------------

namespace Antd.Modules {

    public class TerminalModule : NancyModule {

        public TerminalModule()
            : base("/terminal") {
            this.RequiresAuthentication();

            Get["/"] = x => View["page-terminal"];

            Post["/"] = x => Response.AsJson((string)(Request.Form.Directory == "" ? Terminal.Execute((string)Request.Form.Command) : Terminal.Execute(Request.Form.Command, Request.Form.Directory)));

            Post["/directory"] = x => {
                string directory = Request.Form.Directory;
                string result;
                if (Directory.Exists(directory)) {
                    result = directory + " > ";
                }
                else {
                    result = "0";
                }
                return Response.AsJson(result);
            };

            Post["/directory/parent"] = x => {
                string result;
                if (!Directory.Exists((string)Request.Form.Directory)) {
                    result = "0";
                }
                else {
                    var parent = Directory.GetParent((string)Request.Form.Directory);
                    if (Directory.Exists(parent.FullName)) {
                        result = parent.FullName + " > ";
                    }
                    else {
                        result = "0";
                    }
                }
                return Response.AsJson(result);
            };

            Post["/api"] = x => {
                var cmds = Request.Form.Command.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
                var result = Request.Form.Directory == "" ? Terminal.MultiLine.Execute((string[])cmds) : Terminal.MultiLine.Execute((string[])cmds, (string)Request.Form.Directory);
                return Response.AsJson(result);
            };

            Post["/direct/get"] = x => Response.AsJson(Terminal.Execute((string)Request.Form.Command));

            Post["/direct/post"] = x => Response.AsJson(Terminal.Execute((string)Request.Form.Command));
        }
    }
}