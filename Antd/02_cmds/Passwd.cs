﻿using anthilla.core;
using System;
using System.Linq;

namespace Antd.cmds {
    public class Passwd {

        private const string getentFileLocation = "/usr/bin/getent";
        private const string shadowArg = "shadow";
        private const string passwdArg = "passwd";
        private const string groupArg = "group";
        private const string useraddFileLocation = "/usr/sbin/useradd";
        private const string mkpasswdFileLocation = "/usr/bin/mkpasswd";
        private const string usermodFileLocation = "/usr/sbin/usermod";

        public static SystemUser[] Get() {
            var result = CommonProcess.Execute(getentFileLocation, shadowArg).ToArray();
            var users = new SystemUser[result.Length];
            for(var i = 0; i < result.Length; i++) {
                var currentLine = result[i];
                var currentLineData = currentLine.Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                users[i] = new SystemUser() {
                    Active = true,
                    Alias = currentLineData[0],
                    Password = currentLineData[1]
                };
            }
            return users;
        }

        public static bool Set() {
            var current = Application.CurrentConfiguration.Users.SystemUsers;
            var running = Application.CurrentConfiguration.Users.SystemUsers;
            for(var i = 0; i < current.Length; i++) {
                var currentUser = current[i];
                var runningUser = running.FirstOrDefault(_ => _.Alias == currentUser.Alias);
                if(runningUser == null) {
                    AddUser(currentUser.Alias);
                }
                if(CommonString.AreEquals(currentUser.Password, runningUser.Password) == false) {
                    SetPassword(currentUser.Alias, currentUser.Password);
                }
            }
            return true;
        }

        public static void AddUser(string user) {
            CommonProcess.Do(useraddFileLocation, user);
        }

        public static string HashPasswd(string input) {
            var result = CommonProcess.Execute(mkpasswdFileLocation, input).ToArray();
            return result[0];
        }

        public static void SetPassword(string user, string password) {
            var args = CommonString.Append("-p '", password, "' ", user);
            CommonProcess.Do(useraddFileLocation, user);
        }
    }
}
