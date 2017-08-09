﻿using antdlib.models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using anthilla.core;
using Parameter = antdlib.common.Parameter;

namespace antdlib.config {
    public class NetscanConfiguration {

        private readonly string _filePath = $"{Parameter.AntdCfg}/services/netscan.conf";
        private readonly NetscanSettingModel _settings;

        private static List<NetscanLabelModel> Values() {
            const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            var list = alphabet.Select((t, i) => new NetscanLabelModel { Number = (i + 1).ToString(), Letter = t.ToString(), Label = "" }).ToList();
            return list;
        }

        public NetscanConfiguration() {
            if(!File.Exists(_filePath)) {
                _settings = new NetscanSettingModel { Values = Values() };
            }
            _settings = JsonConvert.DeserializeObject<NetscanSettingModel>(File.ReadAllText(_filePath));
        }

        public NetscanSettingModel Get() {
            return _settings;
        }

        public void Save(NetscanSettingModel model) {
            FileWithAcl.WriteAllText(_filePath, JsonConvert.SerializeObject(model, Formatting.Indented), "644", "root", "wheel");
        }

        #region [    repo    ] 
        public void SetSubnet(string subnet, string label) {
            if(string.IsNullOrEmpty(subnet)) { return; }
            _settings.Subnet = subnet;
            Save(_settings);
        }

        public void SetLabel(string letter, string number, string label) {
            var objects = _settings.Values;
            var list = new List<NetscanLabelModel>();
            foreach(var o in objects) {
                if(o.Number == number && o.Letter == letter) {
                    o.Label = label;
                }
                list.Add(o);
            }
            _settings.Values = list.Count > 25 ? list.Take(26).ToList() : list;
            Save(_settings);
        }
        #endregion

        public void SaveEtcNetworks() {
            //todo ridefinisci questo
            //var settings = _settings.Values.Where(_ => !string.IsNullOrEmpty(_.Label)).Select(
            //    _ => $"{_settings.SubnetLabel}-{_.Label} {_settings.Subnet}{_.Number}.0"
            //);
            //var hostConfiguration = new HostParametersConfiguration();
        }
    }
}
