using BepInEx;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace Erenshor_LevelupSound
{
    [BepInPlugin(ModGUID, ModDescription, ModVersion)]
    public class LevelupSound : BaseUnityPlugin
    {
        internal const string ModName = "LevelupSound";
        internal const string ModVersion = "1.0.0";
        internal const string ModDescription = "Levelup Sound Changer";
        internal const string Author = "Brad522";
        private const string ModGUID = Author + "." + ModName;

        public static AudioClip lvlupSFX;

        public void Awake()
        {
            Logger.LogMessage("Levelup Sound: Started");

            string modDir = Path.GetDirectoryName(Info.Location);
            string[] supportedExtensions = { ".wav", ".mp3", ".ogg" };
            string audioFilePath = Directory
                .EnumerateFiles(modDir)
                .FirstOrDefault(file => supportedExtensions.Contains(Path.GetExtension(file).ToLower()));

            if (!string.IsNullOrEmpty(audioFilePath))
            {
                StartCoroutine(GrabSFX(audioFilePath));
            } else
            {
                Logger.LogMessage("Levelup Sound: No supported audio file found in mod directory.");
            }
        }

        private IEnumerator GrabSFX(string path)
        {
            Logger.LogMessage("Levelup Sound: Attempting to load audio file from: " + path);

            AudioType audioType = GetAudioTypeFromExtension(Path.GetExtension(path));

            if (audioType == AudioType.UNKNOWN)
            {
                Logger.LogMessage("Levelup Sound: Unsupported audio file type, cannot load sound.");
                yield break;
            }

            using (var uwr = UnityWebRequestMultimedia.GetAudioClip("file:///" + path, audioType))
            {
                yield return uwr.SendWebRequest();

                if (uwr.result != UnityWebRequest.Result.Success)
                {
                    Logger.LogMessage("Levelup Sound: UWR failed: " + uwr.error);
                    yield break;
                }

                lvlupSFX = DownloadHandlerAudioClip.GetContent(uwr);
                Logger.LogMessage("Levelup Sound: Audio file loaded successfully: " + path);

                if (GameData.Misc == null)
                    Logger.LogMessage("Levelup Sound: Waiting for GameData.Misc to be initialized...");

                while (GameData.Misc == null)
                    yield return null;

                GameData.Misc.LvlUpSFX = lvlupSFX;
                Logger.LogMessage("Levelup Sound: Custom levelup sound changed.");
            }
        }

        private AudioType GetAudioTypeFromExtension(string extension)
        {
            switch (extension.ToLower())
            {
                case ".wav":
                    return AudioType.WAV;
                case ".mp3":
                    return AudioType.MPEG;
                case ".ogg":
                    return AudioType.OGGVORBIS;
                default:
                    Logger.LogMessage("Levelup Sound: Unsupported audio file type: " + extension);
                    return AudioType.UNKNOWN;
            }
        }
    }
}
