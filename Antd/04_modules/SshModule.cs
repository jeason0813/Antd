﻿using Antd.cmds;
using Nancy;
using Newtonsoft.Json;

namespace Antd.Modules {
    public class SshModule : NancyModule {

        public SshModule() : base("/ssh") {

            //Before += ctx => {
            //    System.Console.WriteLine(Request.Headers.UserAgent);
            //    return null;
            //};

            Get["/authorizedkeys"] = x => {
                return JsonConvert.SerializeObject(Application.CurrentConfiguration.Services.Ssh.AuthorizedKey);
            };

            Get["/publickey"] = x => {
                return Response.AsText(Application.CurrentConfiguration.Services.Ssh.PublicKey);
            };

            Post["/save/authorizedkeys"] = x => {
                string data = Request.Form.Data;
                var objects = JsonConvert.DeserializeObject<AuthorizedKey[]>(data);
                Application.CurrentConfiguration.Services.Ssh.AuthorizedKey = objects;
                ConfigRepo.Save();
                return HttpStatusCode.OK;
            };

            Post["/apply/authorizedkeys"] = x => {
                Ssh.SetAuthorizedKey();
                return HttpStatusCode.OK;
            };
        }
    }
}