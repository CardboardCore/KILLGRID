using System;
using System.Linq;
using UnityEngine;

namespace Attic.Audio
{
    [Serializable]
    public class SFXConfig
    {
        [SerializeField] private string name;
        [SerializeField] private AudioClip sfxClip;

        public string Name => name;
        public AudioClip SFXClip => sfxClip;
    }

    [CreateAssetMenu(fileName = "SFXDatabase", menuName = "WiredDreams/Audio/SFXDatabase")]
    public class SFXDatabase : ScriptableObject
    {
        [SerializeField] private SFXConfig[] sfxConfigs;

        public AudioClip GetSFXClip(string sfxNAme)
        {
            foreach (SFXConfig sfxConfig in sfxConfigs)
            {
                if (sfxConfig.Name == sfxNAme)
                {
                    return sfxConfig.SFXClip;
                }
            }

            Debug.LogError($"SFX with name {sfxNAme} not found in database.");
            return null;
        }

        public AudioClip[] GetSFXClipsContainingName(string musicName)
        {
            return Array.FindAll(sfxConfigs, musicConfig => musicConfig.Name.Contains(musicName))
                .Select(musicConfig => musicConfig.SFXClip)
                .ToArray();
        }
    }
}
