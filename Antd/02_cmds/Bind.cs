﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using anthilla.core;
using Antd.parsing;
using Antd.models;

namespace Antd.cmds {

    public class Bind {

        private const string serviceName = "named.service";
        private const string MainZonesPath = "/etc/bind/zones";
        private const string MainFilePath = "/etc/bind/named.conf";
        private const string MainFilePathBackup = "/etc/bind/.named.conf";
        private const string RndcConfFile = "/etc/bind/rndc.conf";
        private const string RndcKeyFile = "/etc/bind/rndc-key";

        public static void Parse() {
            if(!File.Exists(MainFilePath)) {
                return;
            }
            var content = File.ReadAllText(MainFilePath);
            if(!content.Contains("options")) {
                return;
            }
            var model = new BindModel { Active = false };
            model = BindParser.ParseOptions(model, content);
            model = BindParser.ParseControl(model, content);
            model = BindParser.ParseKeySecret(model, content);
            model = BindParser.ParseLogging(model, content);
            var acls = BindParser.ParseAcl(content).ToArray();
            model.AclList = acls;
            var simpleZone = BindParser.ParseSimpleZones(content).ToList();
            var complexZone = BindParser.ParseComplexZones(content).ToList();
            complexZone.AddRange(simpleZone);
            model.Zones = complexZone;
            var includes = BindParser.ParseInclude(content).ToArray();
            model.IncludeFiles = includes;
            Application.CurrentConfiguration.Services.Bind = model;
            ConsoleLogger.Log("[bind] import existing configuration");
        }

        public static void ParseZoneFile(string filePath) {
            throw new NotImplementedException();
            //var content = File.ReadAllText(@"D:\etc\bind\zones\host.intd01.local.db");
            //var zone = DnsZoneFile.Parse(content);
            //ConsoleLogger.Log(zone.Records.Count);
        }

        public static void DownloadRootServerHits() {
            var text = ApiConsumer.GetString("https://www.internic.net/domain/named.named");
            const string namedHintsFile = "/etc/bind/named.named";
            File.WriteAllText(namedHintsFile, text);
            RndcReload();
        }

        public static void Apply() {
            var options = Application.CurrentConfiguration.Services.Bind;
            if(options == null) {
                return;
            }
            Stop();
            #region [    named.conf generation    ]
            if(File.Exists(MainFilePath)) {
                if(File.Exists(MainFilePathBackup)) {
                    File.Delete(MainFilePathBackup);
                }
                File.Copy(MainFilePath, MainFilePathBackup);
            }
            var lines = new List<string> {
                "options {"
            };
            lines.Add($"notify {options.Notify};");
            lines.Add($"max-cache-size {options.MaxCacheSize};");
            lines.Add($"max-cache-ttl {options.MaxCacheTtl};");
            lines.Add($"max-ncache-ttl {options.MaxNcacheTtl};");
            if(options.Forwarders.Any()) {
                lines.Add("forwarders {");
                foreach(var fwd in options.Forwarders) {
                    lines.Add($"{fwd};");
                }
                lines.Add("}");
            }
            lines.Add($"forwarders {{ {CommonString.Build(options.Forwarders.ToArray(), "; ")} }}");
            lines.Add($"allow-notify {{ {CommonString.Build(options.AllowNotify.ToArray(), "; ")} }}");
            lines.Add($"allow-transfer {{ {CommonString.Build(options.AllowTransfer.ToArray(), "; ")} }}");
            lines.Add($"recursion {options.Recursion};");
            lines.Add($"transfer-format {options.TransferFormat};");
            lines.Add($"query-source address {options.QuerySourceAddress} port {options.QuerySourcePort};");
            lines.Add($"version {options.Version};");
            lines.Add($"allow-query {{ {CommonString.Build(options.AllowQuery.ToArray(), "; ")} }}");
            lines.Add($"allow-recursion {{ {CommonString.Build(options.AllowRecursion.ToArray(), "; ")} }}");
            lines.Add($"ixfr-from-differences {options.IxfrFromDifferences};");
            lines.Add($"listen-on-v6 {{ {CommonString.Build(options.ListenOnV6.ToArray(), "; ")} }}");
            lines.Add($"listen-on port 53 {{ {CommonString.Build(options.ListenOnPort53.ToArray(), "; ")} }}");
            lines.Add($"dnssec-enable {options.DnssecEnabled};");
            lines.Add($"dnssec-validation {options.DnssecValidation};");
            lines.Add($"dnssec-lookaside {options.DnssecLookaside};");
            lines.Add($"auth-nxdomain {options.AuthNxdomain};");
            lines.Add("};");
            lines.Add("");

            lines.Add($"key \"{options.KeyName}\" {{");
            lines.Add("algorithm hmac-md5;");
            lines.Add($"secret \"{options.KeySecret}\";");
            lines.Add("};");
            lines.Add("");

            lines.Add(
                options.ControlKeys.Any()
                    ? $"controls {{ inet {options.ControlIp} port {options.ControlPort} allow {{ {CommonString.Build(options.ControlAllow.ToArray(), "; ")} }} keys {{ {CommonString.Build(options.ControlKeys.Select(_ => "\"" + _ + "\"").ToArray(), ";")} }}"
                    : $"controls {{ inet {options.ControlIp} port {options.ControlPort} allow {{ {CommonString.Build(options.ControlAllow.ToArray(), "; ")} }}");

            lines.Add("");

            foreach(var acl in options.AclList) {
                lines.Add($"acl {acl.Name} {{ {CommonString.Build(acl.InterfaceList.ToArray(), "; ")} }}");
            }
            lines.Add("");

            lines.Add("logging {");
            lines.Add("channel syslog {");
            lines.Add("syslog daemon;");
            lines.Add($"severity {options.SyslogSeverity};");
            lines.Add($"print-category {options.SyslogPrintCategory};");
            lines.Add($"print-severity {options.SyslogPrintSeverity};");
            lines.Add($"print-time {options.SyslogPrintTime};");
            lines.Add("};");
            lines.Add("category client { syslog };");
            lines.Add("category config { syslog };");
            lines.Add("category database { syslog };");
            lines.Add("category default { syslog };");
            lines.Add("category delegation-only { syslog };");
            lines.Add("category dispatch { syslog };");
            lines.Add("category dnssec { syslog };");
            lines.Add("category general { syslog };");
            lines.Add("category lame-servers { syslog };");
            lines.Add("category network { syslog };");
            lines.Add("category notify { syslog };");
            lines.Add("category queries { syslog };");
            lines.Add("category resolver { syslog };");
            lines.Add("category rpz { syslog };");
            lines.Add("category rate-limit { syslog };");
            lines.Add("category security { syslog };");
            lines.Add("category unmatched { syslog };");
            lines.Add("category update { syslog };");
            lines.Add("category update-security { syslog };");
            lines.Add("category xfer-in { syslog };");
            lines.Add("category xfer-out { syslog };");
            lines.Add("};");
            lines.Add("");

            lines.Add("trusted-keys {");
            lines.Add(options.TrustedKeys);
            lines.Add("};");
            lines.Add("");

            //var zones = options.Zones;
            //foreach(var zone in zones) {
            //    lines.Add($"zone \"{zone.Name}\" {{");
            //    lines.Add($"type {zone.Type};");
            //    lines.Add($"file \"{zone.File}\";");
            //    if(!string.IsNullOrEmpty(zone.SerialUpdateMethod)) {
            //        lines.Add($"serial-update-method {zone.SerialUpdateMethod};");
            //    }
            //    if(zone.AllowUpdate.Any()) {
            //        lines.Add($"allow-update {{ {CommonString.Build(zone.AllowUpdate.ToArray(), "; ")} }}");
            //    }
            //    if(zone.AllowQuery.Any()) {
            //        lines.Add($"allow-query {{ {CommonString.Build(zone.AllowQuery.ToArray(), "; ")} }}");
            //    }
            //    if(zone.AllowTransfer.Any()) {
            //        lines.Add($"allow-transfer {{ {CommonString.Build(zone.AllowTransfer.ToArray(), "; ")} }}");
            //        lines.Add($"allow-transfer {zone.AllowTransfer};");
            //    }
            //    lines.Add("};");
            //}
            //lines.Add("");

            lines.Add("include \"/etc/bind/master/blackhole.zones\";");
            File.WriteAllLines(MainFilePath, lines);

            var keyLines = new List<string> {
                $"key \"{options.KeyName}\" {{",
                "algorithm hmac-md5;",
                $"secret \"{options.KeySecret}\";",
                "};",
                ""
            };
            File.WriteAllLines(RndcKeyFile, keyLines);

            var rndcConfLines = new List<string>{
                $"key \"{options.KeyName}\" {{",
                "algorithm hmac-md5;",
                $"secret \"{options.KeySecret}\";",
                "};",
                "",
                "options {",
                $"default-key \"{options.KeyName}\";",
                $"default-server \"{options.ControlIp}\";",
                $"default-port \"{options.ControlPort}\";",
                "};"
            };
            File.WriteAllLines(RndcConfFile, rndcConfLines);

            #endregion
            Start();
            RndcReconfig();
        }

        public static void Stop() {
            Systemctl.Stop(serviceName);
            ConsoleLogger.Log("[bind] stop");
        }

        public static void Start() {
            if(Systemctl.IsEnabled(serviceName) == false) {
                Systemctl.Enable(serviceName);
            }
            if(Systemctl.IsActive(serviceName) == false) {
                Systemctl.Restart(serviceName);
            }
            ConsoleLogger.Log("[bind] start");
        }

        public static void RndcReconfig() {
            Bash.Execute("rndc reconfig");
        }

        public static void RndcReload() {
            Bash.Execute("rndc reload");
        }

        public static List<string> GetHostZoneText(string hostname, string domain, string ip) {
            var list = new List<string> {"$ORIGIN .",
                "$TTL 3600	; 1 hour",
                $"{domain}			IN SOA	{hostname}.{domain}. hostmaster.{domain}. (",
                "				1000	   ; serial",
                "				900        ; refresh (15 minutes)",
                "				600        ; retry (10 minutes)",
                "				86400      ; expire (1 day)",
                "				3600       ; minimum (1 hour)",
                "				)",
                $"			NS	{hostname}.{domain}.",
                "$TTL 600	; 10 minutes",
                $"			A	{ip}",
                $"$ORIGIN _tcp.DefaultSite._sites.{domain}.",
                $"_gc			SRV	0 100 3268 {hostname}.{domain}.",
                $"_kerberos		SRV	0 100 88 {hostname}.{domain}.",
                $"_ldap			SRV	0 100 389 {hostname}.{domain}.",
                $"$ORIGIN _tcp.{domain}.",
                $"_gc			SRV	0 100 3268 {hostname}.{domain}.",
                $"_kerberos		SRV	0 100 88 {hostname}.{domain}.",
                $"_kpasswd		SRV	0 100 464 {hostname}.{domain}.",
                $"_ldap			SRV	0 100 389 {hostname}.{domain}.",
                $"$ORIGIN _udp.{domain}.",
                $"_kerberos		SRV	0 100 88 {hostname}.{domain}.",
                $"_kpasswd		SRV	0 100 464 {hostname}.{domain}.",
                $"$ORIGIN {domain}.",
                "$TTL 600	; 10 minutes",
                $"domaindnszones		A	{ip}",
                $"$ORIGIN domaindnszones.{domain}.",
                $"_ldap._tcp.DefaultSite._sites	SRV	0 100 389 {hostname}.{domain}.",
                $"_ldap._tcp		SRV	0 100 389 {hostname}.{domain}.",
                $"$ORIGIN {domain}.",
                "$TTL 1200	; 20 minutes",
                $"forestdnszones		A	{ip}",
                $"$ORIGIN forestdnszones.{domain}.",
                $"_ldap._tcp.DefaultSite._sites	SRV	0 100 389 {hostname}.{domain}.",
                $"_ldap._tcp		SRV	0 100 389 {hostname}.{domain}.",
                $"$ORIGIN _tcp.DefaultSite._sites.dc._msdcs.{domain}.",
                "$TTL 3600       ; 1 hour",
                $"_kerberos		SRV	0 100 88 {hostname}.{domain}.",
                $"_ldap			SRV	0 100 389 {hostname}.{domain}.",
                $"$ORIGIN _tcp.dc._msdcs.{domain}.",
                "$TTL 3600       ; 1 hour",
                $"_kerberos		SRV	0 100 88 {hostname}.{domain}.",
                $"_ldap			SRV	0 100 389 {hostname}.{domain}.",
                $"$ORIGIN _msdcs.{domain}.",
                "$TTL 3600       ; 1 hour",
                $"_ldap._tcp		SRV 0 100 389 {hostname}.{domain}.",
                $"$ORIGIN gc._msdcs.{domain}.",
                "$TTL 3600       ; 1 hour",
                $"_ldap._tcp.DefaultSite._sites SRV	0 100 3268 {hostname}.{domain}.",
                $"_ldap._tcp		SRV	0 100 3268 {hostname}.{domain}.",
                $"$ORIGIN _msdcs.{domain}.",
                "$TTL 3600       ; 1 hour",
                $"_ldap._tcp.pdc		SRV	0 100 389 {hostname}.{domain}.",
                $"$ORIGIN {domain}.",};
            return list;
        }

        public static List<string> GetReverseZoneText(string hostname, string domain, string arpaNet, string arpaIp) {
            var list = new List<string> {
                "$ORIGIN .",
                "$TTL 3600	; 1 hour",
                $"{arpaNet}.in-addr.arpa   IN SOA	{hostname}.{domain}. hostmaster.{domain}. (",
                "				1111	   ; serial",
                "				900        ; refresh (15 minutes)",
                "				600        ; retry (10 minutes)",
                "				86400      ; expire (1 day)",
                "				3600       ; minimum (1 hour)",
                "				900        ; refresh (15 minutes)",
                "				)",
                $"			NS  {hostname}.{domain}.",
                $"$ORIGIN {arpaNet}.in-addr.arpa.",
                $"{arpaIp} PTR	{hostname}.{domain}."
            };

            return list;
        }

        public class Verbs {

            public static string[] ZoneType = new string[] {
                "master",
                "slave"
            };

            public static string[] ZoneRecordType = new string[] {
                "ip",
                "ip6",
                "inet",
                "arp",
                "bridge"
            };

            public static string[] ZoneRecordCaaFlag = new string[] {
                "0",
                "1"
            };

            public static string[] ZoneRecordCaaTag = new string[] {
                "issue",
                "issuewild",
                "iodef"
            };
        }
    }
}