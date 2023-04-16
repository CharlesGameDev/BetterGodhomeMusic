using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using GlobalEnums;
using IL.InControl.UnityDeviceProfiles;
using Modding;
using Modding.Delegates;
using UnityEngine;
using WavLib;
using Random = UnityEngine.Random;

namespace RemoveHauntedFoes
{
    public class RemoveHauntedFoes : Mod, IMenuMod
    {
        public bool ToggleButtonInsideMenu => throw new NotImplementedException();
        string dir;
        string musicDir;
        public static Dictionary<string, AudioClip> musicDict;
        public static Dictionary<string, AudioClip> soundDict;
        public static Dictionary<string, Texture2D> imageDict;
        string currentScene;

        new public string GetName() => "Remove Haunted Foes";
        public override string GetVersion() => "v1.0.0";
        public override void Initialize()
        {
            Log("Loading resources");
            dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            musicDir = dir + "\\Resources\\Music";
            DebugLog("Resources folder: " + dir);
            DebugLog("Resources/Music folder: " + musicDir);
            musicDict = new Dictionary<string, AudioClip>();
            foreach (string file in Directory.GetFiles(musicDir))
            {
                string newFile = Path.GetFileNameWithoutExtension(file);
                musicDict.Add(newFile, GetAudioClip(musicDir, newFile));
            }

            ModHooks.BeforeSceneLoadHook += BeforeSceneLoad;
            On.HeroController.Update += HeroUpdate;
            On.AudioManager.ApplyMusicCue += ApplyMusicCue;
        }

        void ApplyMusicCue(On.AudioManager.orig_ApplyMusicCue orig, AudioManager self, MusicCue musicCue, float delayTime, float transitionTime, bool applySnapshot)
        {
            DebugLog("Music cue: " + musicCue.name);
            if ((musicCue.name == "GG Sad" || musicCue.name == "NightmareGrimm") && currentScene == "GG_Grimm_Nightmare")
            {
                PlayAudioClip(musicDict["nkg"], HeroController.instance.transform.position, 0.8f);
                return;
            }
            if ((musicCue.name == "GG Sad" || musicCue.name == "Grimm") && currentScene == "GG_Grimm")
            {
                PlayAudioClip(musicDict["nkg"], HeroController.instance.transform.position, 0.8f);
                return;
            }
            if ((musicCue.name == "GG Sad" || musicCue.name == "MimicSpider") && currentScene == "GG_Nosk_Hornet")
            {
                PlayAudioClip(musicDict["nosk"], HeroController.instance.transform.position, 0.8f);
                return;
            }
            orig(self, musicCue, delayTime, transitionTime, applySnapshot);
        }

        public void HeroUpdate(On.HeroController.orig_Update orig, HeroController self)
        {
            orig(self);
        }

        public List<IMenuMod.MenuEntry> GetMenuData(IMenuMod.MenuEntry? toggleButtonEntry)
        {
            return new List<IMenuMod.MenuEntry>
            {
            };
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
                //return WavUtility.ToAudioClip($"{_dir}/{origName}.wav");
            }

            DebugLog($"File not found for \"{origName}\"");
            return null;
        }

        public void PlayAudioClip(AudioClip clip, Vector3 pos, float volume = 1)
        {
            DebugLog("PLAYING AUDIO CLIP: " + clip.name + " AT " + pos.ToString() + " WITH VOLUME " + volume);
            GameObject o = new(clip.name);
            o.transform.position = pos;
            AudioSource source = o.AddComponent<AudioSource>();
            source.volume = volume;
            source.loop = true;
            source.clip = clip;
            source.Play();
        }

        public string BeforeSceneLoad(string newSceneName)
        {
            currentScene = newSceneName;
            Log($"new scene is {newSceneName}");
            return newSceneName;
        }

        public static void DebugLog(string msg)
        {
            Modding.Logger.Log(msg);
            Debug.Log("[KnightsBizarreAdevnture] - " + msg);
        }
    }
}
