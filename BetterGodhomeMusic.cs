﻿#define DEBUG_MESSAGES

using Modding;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using WavLib;

namespace BetterGodhomeMusic
{
    public class BetterGodhomeMusic : Mod, IMenuMod
    {
        public bool ToggleButtonInsideMenu => throw new NotImplementedException();
        string dir;
        string musicDir;
        public static Dictionary<string, AudioClip> musicDict;
        public static Dictionary<string, AudioClip> soundDict;
        public static Dictionary<string, Texture2D> imageDict;
        string currentScene;
        bool GRIMM = true;
        bool NKG = true;
        bool NOSK = true;
        bool ABSRAD = true;
        bool HIVEKNIGHT = true;
        bool FK = true;
        bool LK = true;
        bool PV = true;

        new public string GetName() => "BetterGodhomeMusic";
        public override string GetVersion() => Assembly.GetAssembly(typeof(BetterGodhomeMusic)).GetName().Version.ToString();
        public override void Initialize()
        {
            dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            musicDir = Directory.Exists(dir + "\\Resources\\Music") ? dir + "\\Resources\\Music" : dir + "/Resources/Music";
            musicDict = new Dictionary<string, AudioClip>();
            foreach (string file in Directory.GetFiles(musicDir))
            {
                string newFile = Path.GetFileNameWithoutExtension(file);
                musicDict.Add(newFile, GetAudioClip(musicDir, newFile));
            }

            ModHooks.BeforeSceneLoadHook += BeforeSceneLoad;
            On.AudioManager.BeginApplyMusicCue += BeginApplyMusicCue;
        }

        public List<IMenuMod.MenuEntry> GetMenuData(IMenuMod.MenuEntry? toggleButtonEntry)
        {
            return new List<IMenuMod.MenuEntry>
            {
                new IMenuMod.MenuEntry {
                    Name = "Troupe Master Grimm",
                    Description = "Use new Troupe Master Grimm music",
                    Values = new string[] {
                        "Off",
                        "On"
                    },
                    Saver = opt => GRIMM = opt switch {
                        0 => false,
                        1 => true,
                        _ => throw new InvalidOperationException()
                    },
                    Loader = () => GRIMM switch {
                        false => 0,
                        true => 1,
                    }
                },
                new IMenuMod.MenuEntry {
                    Name = "Nightmare King Grimm",
                    Description = "Use new NKG music",
                    Values = new string[] {
                        "Off",
                        "On"
                    },
                    Saver = opt => NKG = opt switch {
                        0 => false,
                        1 => true,
                        _ => throw new InvalidOperationException()
                    },
                    Loader = () => NKG switch {
                        false => 0,
                        true => 1,
                    }
                },
                new IMenuMod.MenuEntry {
                    Name = "Nosk",
                    Description = "Use new Nosk music",
                    Values = new string[] {
                        "Off",
                        "On"
                    },
                    Saver = opt => NOSK = opt switch {
                        0 => false,
                        1 => true,
                        _ => throw new InvalidOperationException()
                    },
                    Loader = () => NOSK switch {
                        false => 0,
                        true => 1,
                    }
                },
                new IMenuMod.MenuEntry {
                    Name = "Absolute Radiance",
                    Description = "Use new Absolute Radiance music",
                    Values = new string[] {
                        "Off",
                        "On"
                    },
                    Saver = opt => ABSRAD = opt switch {
                        0 => false,
                        1 => true,
                        _ => throw new InvalidOperationException()
                    },
                    Loader = () => ABSRAD switch {
                        false => 0,
                        true => 1,
                    }
                },
                new IMenuMod.MenuEntry {
                    Name = "Hive Knight",
                    Description = "Use new Hive Knight music",
                    Values = new string[] {
                        "Off",
                        "On"
                    },
                    Saver = opt => HIVEKNIGHT = opt switch {
                        0 => false,
                        1 => true,
                        _ => throw new InvalidOperationException()
                    },
                    Loader = () => HIVEKNIGHT switch {
                        false => 0,
                        true => 1,
                    }
                },
                new IMenuMod.MenuEntry {
                    Name = "Failed Champion",
                    Description = "Use new False Knight/Failed Champion music",
                    Values = new string[] {
                        "Off",
                        "On"
                    },
                    Saver = opt => FK = opt switch {
                        0 => false,
                        1 => true,
                        _ => throw new InvalidOperationException()
                    },
                    Loader = () => FK switch {
                        false => 0,
                        true => 1,
                    }
                },
                new IMenuMod.MenuEntry {
                    Name = "Lost Kin",
                    Description = "Use new Broken Vessel/Lost Kin music",
                    Values = new string[] {
                        "Off",
                        "On"
                    },
                    Saver = opt => LK = opt switch {
                        0 => false,
                        1 => true,
                        _ => throw new InvalidOperationException()
                    },
                    Loader = () => LK switch {
                        false => 0,
                        true => 1,
                    }
                },
                new IMenuMod.MenuEntry {
                    Name = "Pure Vessel",
                    Description = "Use new Pure Vessel music",
                    Values = new string[] {
                        "Off",
                        "On"
                    },
                    Saver = opt => PV = opt switch {
                        0 => false,
                        1 => true,
                        _ => throw new InvalidOperationException()
                    },
                    Loader = () => PV switch {
                        false => 0,
                        true => 1,
                    }
                },
            };
        }

        IEnumerator BeginApplyMusicCue(On.AudioManager.orig_BeginApplyMusicCue orig, AudioManager self, MusicCue musicCue, float delayTime, float transitionTime, bool applySnapshot)
        {
            DebugLog("Music cue: " + musicCue.name);

            MusicCue.MusicChannelInfo[] infos = ReflectionHelper.GetField<MusicCue, MusicCue.MusicChannelInfo[]>(musicCue, "channelInfos");
            bool changed = false;

            foreach (MusicCue.MusicChannelInfo info in infos)
            {
                AudioClip possibleReplace = GetAudioClip(musicCue.name);
                if (possibleReplace != null || musicCue.name == "None")
                {
                    ReflectionHelper.SetField(info, "clip", possibleReplace);
                    changed = true;
                }
            }

            if (changed)
                ReflectionHelper.SetField(musicCue, "channelInfos", infos);

            yield return orig(self, musicCue, delayTime, transitionTime, applySnapshot);
        }

        public AudioClip GetAudioClip(string name)
        {
            if (NKG && ((name == "GG Sad" || name == "NightmareGrimm") && currentScene == "GG_Grimm_Nightmare")) return musicDict["nkg"];
            if (GRIMM && ((name == "GG Sad" || name == "Grimm") && currentScene == "GG_Grimm")) return musicDict["grimm"];
            if (NOSK && ((name == "GG Sad" || name == "MimicSpider") && (currentScene == "GG_Nosk_Hornet" || currentScene == "GG_Nosk"))) return musicDict["nosk"];
            if (ABSRAD && name == "Radiance" && currentScene == "GG_Radiance") return musicDict["absrad"];
            if (HIVEKNIGHT && (name == "HiveKnight" || name == "GG Normal") && currentScene == "GG_Hive_Knight") return musicDict["hiveknight"];
            if (FK && (name == "FalseKnight" || name == "Boss1" || name == "GG Heavy") && (currentScene == "GG_Failed_Champion" || currentScene == "GG_False_Knight")) return musicDict["falseknight"];
            if (LK && (name == "BossIK" || name == "None") && (currentScene == "GG_Lost_Kin" || currentScene == "GG_Broken_Vessel")) return musicDict["lostkin"];
            if (PV && name == "HollowKnightPrime" && currentScene == "GG_Hollow_Knight") return musicDict["purevessel"];

            return null;
        }

        private AudioClip GetAudioClip(string dir, string origName)
        {
            if (File.Exists($"{dir}/{origName}.wav"))
            {
                DebugLog($"Using audio file \"{origName}.wav\"");
                FileStream stream = File.OpenRead($"{dir}/{origName}.wav");
                WavData.Inspect(stream, null);
                WavData wavData = new();
                wavData.Parse(stream);
                stream.Close();

                /*
                DebugLog($"{origName} - AudioFormat: {wavData.FormatChunk.AudioFormat}");
                DebugLog($"{origName} - NumChannels: {wavData.FormatChunk.NumChannels}");
                DebugLog($"{origName} - SampleRate: {wavData.FormatChunk.SampleRate}");
                DebugLog($"{origName} - ByteRate: {wavData.FormatChunk.ByteRate}");
                DebugLog($"{origName} - BlockAlign: {wavData.FormatChunk.BlockAlign}");
                DebugLog($"{origName} - BitsPerSample: {wavData.FormatChunk.BitsPerSample}");
                */

                float[] wavSoundData = wavData.GetSamples();
                AudioClip audioClip = AudioClip.Create(origName, wavSoundData.Length / wavData.FormatChunk.NumChannels, wavData.FormatChunk.NumChannels, (int)wavData.FormatChunk.SampleRate, false);
                audioClip.SetData(wavSoundData, 0);
                DebugLog($"Loaded \"{origName}.wav\"");
                return audioClip;
            }

            DebugLog($"File not found for \"{origName}\"");
            return null;
        }

        public string BeforeSceneLoad(string newSceneName)
        {
            currentScene = newSceneName;
            DebugLog($"new scene is {newSceneName}");
            return newSceneName;
        }

        public void DebugLog(string msg)
        {
#if DEBUG_MESSAGES
            Log(msg);
            Debug.Log("[BetterGodhomeMusic] - " + msg);
#endif
        }
    }
}
