﻿using Newtonsoft.Json;
using System.Net;
using System.Text.Json;

namespace Chatterino.Common
{
    public class Account
    {
        public string Username { get; set; }
        public string OauthToken { get; set; }
        public string ClientId { get; set; }
        public string Scope { get; set; }
        private string userid;
        public string UserId {
            get {
                if (userid != null) {
                    return userid;
                } else {
                    loadUserIDFromTwitch(this, Username, ClientId);
                    return userid;
                }
            }
            set {
                userid = value;
            }
        }
        [JsonIgnore]
        public bool IsAnon { get; private set; }

        public Account(string username, string oauthToken, string clientId, string scope)
        {
            Username = username;
            OauthToken = oauthToken;
            ClientId = clientId;
            Scope = scope;
            loadUserIDFromTwitch(this, username, clientId);
        }
        protected bool loadUserIDFromTwitch(Account account, string username, string clientId)
        {
            // call twitch api
            if (username != "" && clientId != string.Empty) {
                try
                {
                    var request =
                    WebRequest.Create(
                        $"https://api.twitch.tv/helix/users?&login={username}");
                    if (AppSettings.IgnoreSystemProxy)
                    {
                        request.Proxy = null;
                    }
                    request.Headers["Authorization"]=$"Bearer {account.OauthToken}";
                    request.Headers["Client-ID"]=$"{clientId}";
                    using (var response = request.GetResponse()) {
                        using (var stream = response.GetResponseStream())
                        {
                            var parser = new JsonParser();
                            dynamic json = parser.Parse(stream);
                            
                            account.UserId = json["users"][0]["_id"];
                        }
                        response.Close();
                    }
                }
                catch
                {
                } 
            }
            return false;
        }
        
        public Account()
        {
            
        }

        public static Account AnonAccount { get; } = new Account("Anon", "", "", "") { IsAnon = true};
    }
}
