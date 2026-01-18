using System;
using UnityEngine;

namespace Attic.Audio
{
    [Serializable]
    public class MusicConfig
    {
        [SerializeField] private string name;
        [SerializeField] private AudioClip musicClip;

        public string Name => name;
        public AudioClip MusicClip => musicClip;
    }

    [CreateAssetMenu(fileName = "MusicDatabase", menuName = "WiredDreams/Audio/MusicDatabase")]
    public class MusicDatabase : ScriptableObject
    {
        [SerializeField] private MusicConfig[] musicConfigs;

        public AudioClip GetMusicClip(string musicName)
        {
            foreach (MusicConfig musicConfig in musicConfigs)
            {
                if (musicConfig.Name == musicName)
                {
                    return musicConfig.MusicClip;
                }
            }

            Debug.LogError($"Music with name {musicName} not found in database.");
            return null;
        }
    }
}
