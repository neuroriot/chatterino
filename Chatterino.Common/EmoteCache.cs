﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Text.Json;
using Newtonsoft.Json;
using System.Drawing;
using System.Web;

namespace Chatterino.Common
{
    
    public static class EmoteCache
    {
        private struct _emotes_cache {
            public string emotePath;
            public bool usedLastTime;
            public bool isAnimated;
            [JsonIgnore]
            public ChatterinoImage emote;
        }
        
        public delegate void EmoteCallback(ChatterinoImage emote);
        
        private static ConcurrentDictionary<string, _emotes_cache> CachedEmotes =
            new ConcurrentDictionary<string, _emotes_cache>();
            
        public static bool CheckEmote(string url) {
            if (AppSettings.CacheEmotes) {
                return CachedEmotes.ContainsKey(url);
            }
            return false;
        }
        
        public static bool IsEmoteLoaded(string url) {
            if (AppSettings.CacheEmotes) {
                _emotes_cache ecache;
                if (CachedEmotes.TryGetValue(url, out ecache)) {
                    return (ecache.emote != null);
                }
            }
            return false;
        }

        //async
        public static void GetEmote(string url, EmoteCallback callback) {
            if (AppSettings.CacheEmotes) {
                Task.Run((() =>
                {
                    _emotes_cache ecache;
                    ecache.emote = null;
                    Mutex m;
                    
                    using(m = new Mutex(false, url)) {
                        m.WaitOne();
                        if (CachedEmotes.TryGetValue(url, out ecache)) {
                            ecache.usedLastTime = true;
                            if (ecache.emote == null && File.Exists(ecache.emotePath)) {
                                try {
                                    MemoryStream mem = new MemoryStream();
                                    using (FileStream stream = new FileStream(ecache.emotePath, FileMode.Open, FileAccess.Read)) {
                                        stream.CopyTo(mem);
                                        ecache.emote = ChatterinoImage.FromStream(mem);
                                    }
                                } catch (Exception e) {
                                    GuiEngine.Current.log("emote faild to load " + ecache.emotePath + " " +e.ToString());
                                    ecache.emote = null;
                                }
                            }
                            if (ecache.emote == null) {
                                //assume file doesnt exist. remove from the cache.
                                CachedEmotes.TryRemove(url, out ecache);
                            } else {
                                CachedEmotes[url]=ecache;
                            }
                        }
                        m.ReleaseMutex();
                        callback(ecache.emote);
                    }
                }));
            } else {
                callback(null);
            }
        }
        
        //sync
        public static ChatterinoImage GetEmoteSync(string url) {
            if (AppSettings.CacheEmotes) {
                _emotes_cache ecache;
                ChatterinoImage ret = null;
                Mutex m;
                using(m = new Mutex(false, url)){
                    m.WaitOne();
                    if (CachedEmotes.TryGetValue(url, out ecache)) {
                        ecache.usedLastTime = true;
                        if (ecache.emote == null && File.Exists(ecache.emotePath)) {
                            try {
                                MemoryStream mem = new MemoryStream();
                                using (FileStream stream = new FileStream(ecache.emotePath, FileMode.Open, FileAccess.Read)) {
                                    stream.CopyTo(mem);
                                    ecache.emote = ChatterinoImage.FromStream(mem);
                                }
                            } catch (Exception e) {
                                GuiEngine.Current.log("emote faild to load " + ecache.emotePath + " " +e.ToString());
                                ecache.emote = null;
                            }
                        }
                        if (ecache.emote == null) {
                            //assume file doesnt exist. remove from the cache.
                            CachedEmotes.TryRemove(url, out ecache);
                        } else {
                            CachedEmotes[url]=ecache;
                        }
                        ret =  ecache.emote;
                    }
                    m.ReleaseMutex();
                }
                return ret;
            }
            return null;
        }
        
        //async
        public static void AddEmote(string url, ChatterinoImage emote) {
            if (AppSettings.CacheEmotes) {
                Task.Run((() =>
                {
                    _emotes_cache ecache;
                    Mutex m;
                    using(m = new Mutex(false, url)) {
                        m.WaitOne();
                        if (!CachedEmotes.ContainsKey(url)) {
                            ecache.usedLastTime = true;
                            ecache.isAnimated = false;
                            ecache.emotePath = Path.Combine(Util.GetUserDataPath(), "Cache", "Emotes", HttpUtility.UrlEncode(url));
                            ecache.emote = emote;
                            try {
                                //save emote to a file
                                if (File.Exists(ecache.emotePath)) {
                                    File.Delete(ecache.emotePath);
                                }
                                lock (emote) {
                                    bool animated = emote.IsAnimated;
                                    if (!animated) {
                                        emote.Save(ecache.emotePath);
                                    } else {
                                        ecache.isAnimated = true;
                                    }
                                }
                                CachedEmotes.TryAdd(url, ecache);
                            } catch (Exception e) {
                                GuiEngine.Current.log("emote faild to save " + ecache.emotePath + " " +e.ToString());
                            }
                        }
                        m.ReleaseMutex();
                    }
                }));
            }
        }
        
        public static void SaveEmoteList() {
            if (AppSettings.CacheEmotes) {
                try {
                    JsonSerializer serializer = new JsonSerializer();
                    string emotesCache = Path.Combine(Util.GetUserDataPath(), "Cache", "emote_cache.json");
                    using(StreamWriter stream = new StreamWriter(emotesCache)) {
                        using (JsonWriter writer = new JsonTextWriter(stream)) {
                            serializer.Serialize(writer, CachedEmotes);
                            writer.Close();
                        }
                        stream.Close();
                    }
                } catch (Exception e) {
                    GuiEngine.Current.log("error saving emote cache " + e.ToString());
                }
            }
        }
        
        public static void init () {
            //load list of emotes from file and delete unused ones
            try {
                string dir = Path.Combine(Util.GetUserDataPath(), "Cache", "Emotes");
                Directory.CreateDirectory(dir);
                string emotesCache = Path.Combine(Util.GetUserDataPath(), "Cache", "emote_cache.json");
                var stream = File.OpenRead(emotesCache);
                var parser = new JsonParser();
                dynamic json = parser.Parse(stream);
                dynamic cachedemote;
                _emotes_cache ecache;
                
                foreach (var url in json.Keys) {
                    cachedemote = json[url];
                    ecache = new _emotes_cache();
                    ecache.usedLastTime = cachedemote["usedLastTime"];
                    ecache.isAnimated = cachedemote["isAnimated"];
                    ecache.emotePath = cachedemote["emotePath"];
                    if (ecache.usedLastTime && !ecache.isAnimated) {
                        ecache.usedLastTime = false;
                        ecache.emote = null;
                        CachedEmotes.TryAdd(url, ecache);
                    } else {
                        //unused delete the cache
                        try {
                            if (File.Exists(ecache.emotePath)) {
                                File.Delete(ecache.emotePath);
                            }
                        } catch (Exception e) {
                            GuiEngine.Current.log("emote faild to delete " + ecache.emotePath + " " +e.ToString());
                        }
                    }
                }
                
                stream.Close();
            } catch (Exception e) {
                GuiEngine.Current.log("error loading emote cache " + e.ToString());
            }
        }
    }
}
