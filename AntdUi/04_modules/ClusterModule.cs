﻿using anthilla.core;
using Nancy;
using Nancy.Security;
using System.Collections.Generic;

namespace AntdUi.Modules {
    public class ClusterModule : NancyModule {

        public ClusterModule() : base("/cluster") {
            //this.RequiresAuthentication();

            Get["/"] = x => {
                return ApiConsumer.GetJson(CommonString.Append(Application.ServerUrl, Request.Path));
            };

            Post["/save"] = x => {
                string data = Request.Form.Data;
                ConsoleLogger.Log("");
                ConsoleLogger.Log("save (sto salvando le info del cluster dalla macchina stessa)");
                ConsoleLogger.Log(data);
                ConsoleLogger.Log("");
                var dict = new Dictionary<string, string> {
                    { "Data", data }
                };
                return ApiConsumer.Post(CommonString.Append(Application.ServerUrl, Request.Path), dict);
            };

            Post["/import"] = x => {
                string data = Request.Form.Data;
                ConsoleLogger.Log("");
                ConsoleLogger.Log("import (sto salvando le info ricevute da un altro nodo)");
                ConsoleLogger.Log(data);
                ConsoleLogger.Log("");
                var dict = new Dictionary<string, string> {
                    { "Data", data }
                };
                return ApiConsumer.Post(CommonString.Append(Application.ServerUrl, Request.Path), dict);
            };

            /// <summary>
            /// Inizia ANCHE la procedura di "condivisione della configurazione nel cluster"
            /// In questo contesto passerà SOLO la configurazione relativa al cluster stesso
            /// Questa API viene richiesta dall'utente (tramite GUI)
            /// </summary>
            Post["/apply"] = x => {
                ConsoleLogger.Log("");
                ConsoleLogger.Log("apply (sto applicando le modifiche del cluster)");
                ConsoleLogger.Log("");
                return ApiConsumer.Post(CommonString.Append(Application.ServerUrl, Request.Path));
            };

            Post["/deploy"] = x => {
                ConsoleLogger.Log("");
                ConsoleLogger.Log("deploy (sto inviando altrove le modifiche del cluster)");
                ConsoleLogger.Log("");
                return ApiConsumer.Post(CommonString.Append(Application.ServerUrl, Request.Path));
            };

            #region [    Handshake + cluster init    ]
            Post["/handshake"] = x => {
                string apple = Request.Form.ApplePie;
                var dict = new Dictionary<string, string> {
                    { "ApplePie", apple }
                };
                return ApiConsumer.Post(CommonString.Append(Application.ServerUrl, Request.Path), dict);
            };

            Post["Add Device to Cluster", "/handshake/begin"] = x => {
                var conf = Request.Form.Data;
                var dict = new Dictionary<string, string> {
                    { "Data", conf }
                };
                return ApiConsumer.Post(CommonString.Append(Application.ServerUrl, Request.Path), dict);
            };
            #endregion
        }
    }
}