using System;
using System.Collections.Generic;
using Attic.DI;
using Attic.Utilities;
using DG.Tweening;
using JetBrains.Annotations;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Attic.Audio
{
    public enum FadeMode
    {
        None,
        Fade
    }

    [Serializable]
    public class PlayMusicOptions
    {
        public float Volume { get; }
        public bool Loop { get; }
        public FadeMode Fade { get; }
        public float FadeDuration { get; }
        public Action OnComplete { get; }

        public PlayMusicOptions(float volume = 1f, bool loop = false, FadeMode fade = FadeMode.None, float fadeDuration = 1f, Action onComplete = null)
        {
            Volume = volume;
            Loop = loop;
            Fade = fade;
            FadeDuration = fadeDuration;
            OnComplete = onComplete;
        }
    }

    public struct StopMusicOptions
    {
        public FadeMode Fade { get; }
        public float FadeDuration { get; }
        public Action OnComplete { get; }

        public StopMusicOptions(FadeMode fade = FadeMode.None, float fadeDuration = 1f, Action onComplete = null)
        {
            Fade = fade;
            FadeDuration = fadeDuration;
            OnComplete = onComplete;
        }
    }

    public struct PlaySFXOptions
    {
        public Vector3? Position { get; }
        [CanBeNull] public Transform Parent { get; }
        public float Volume { get; }
        public float Pitch { get; }
        public bool IsRandomized { get; }

        private PlaySFXOptions(float volume = 1f, float pitch = 1f, bool isRandomized = false)
        {
            Position = null;
            Parent = null;
            Volume = volume;
            Pitch = pitch;
            IsRandomized = isRandomized;
        }

        public PlaySFXOptions(Vector3? position = null, float volume = 1f, float pitch = 1f, bool isRandomized = false)
        {
            Position = position;
            Parent = null;
            Volume = volume;
            Pitch = pitch;
            IsRandomized = isRandomized;
        }

        public PlaySFXOptions([CanBeNull] Transform parent = null, float volume = 1f, float pitch = 1f, bool isRandomized = false)
        {
            Position = null;
            Parent = parent;
            Volume = volume;
            Pitch = pitch;
            IsRandomized = isRandomized;
        }

        public static PlaySFXOptions Default()
        {
            return new PlaySFXOptions(1f, 1f, false);
        }
    }

    [Injectable]
    public class AudioManager : CardboardCoreBehaviour
    {
        [Header("Databases")]
        [SerializeField] private SFXDatabase sfxDatabase;
        [SerializeField] private MusicDatabase musicDatabase;

        [Header("SFX")]
        [SerializeField] private WiredDreamsAudioSource sfxAudioSourcePrefab;

        [Header("Music")]
        [SerializeField] private AudioSource musicAudioSource;


        private List<AudioSource> oneShotAudioSources;

        protected override void OnInjected()
        {

        }

        protected override void OnReleased()
        {

        }

        public void PlaySFX(string sfxName, PlaySFXOptions options)
        {
            if (sfxDatabase == null)
            {
                return;
            }

            AudioClip sfxClip = options.IsRandomized ? null : sfxDatabase.GetSFXClip(sfxName);

            if (options.IsRandomized)
            {
                // Find all the music clips which contain the random music name
                AudioClip[] sfxClips = sfxDatabase.GetSFXClipsContainingName(sfxName);

                if (sfxClips.Length == 0)
                {
                    Log.Warn($"Unable to play SFX with name {sfxName}. SFX clip not found in database.");
                    return;
                }

                // Choose a random music clip from the list
                sfxClip = sfxClips[Random.Range(0, sfxClips.Length)];
            }

            if (sfxClip == null)
            {
                Log.Warn($"Unable to play SFX with name {sfxName}. SFX clip not found in database.");
                return;
            }

            Instantiate(sfxAudioSourcePrefab).Play(sfxClip, options);
        }

        public void PlayMusic(string musicName, PlayMusicOptions options)
        {
            AudioClip musicClip = musicDatabase.GetMusicClip(musicName);

            if (musicClip == null)
            {
                Log.Warn($"Unable to play music with name {musicName}. Music clip not found in database.");
                return;
            }

            bool isNewMusic = musicAudioSource.clip != musicClip;

            if (isNewMusic)
            {
                if (musicAudioSource.isPlaying)
                {
                    musicAudioSource.DOFade(0f, options.FadeDuration).OnComplete(() =>
                    {
                        musicAudioSource.Stop();

                        FadeNewMusicIn();

                        musicAudioSource.clip = musicClip;
                        musicAudioSource.Play();
                    });
                }
                else
                {
                    musicAudioSource.clip = musicClip;

                    FadeNewMusicIn();

                    musicAudioSource.Play();
                }
            }

            musicAudioSource.loop = options.Loop;

            return;

            void FadeNewMusicIn()
            {
                switch (options.Fade)
                {
                    case FadeMode.None:
                        musicAudioSource.volume = options.Volume;
                        break;

                    case FadeMode.Fade:
                        musicAudioSource.volume = 0f;
                        musicAudioSource.DOFade(options.Volume, options.FadeDuration).OnComplete(() => options.OnComplete?.Invoke());
                        break;
                }
            }
        }

        public void StopCurrentMusic(StopMusicOptions options)
        {
            switch (options.Fade)
            {
                case FadeMode.None:
                    musicAudioSource.Stop();
                    musicAudioSource.clip = null;
                    options.OnComplete?.Invoke();
                    break;

                case FadeMode.Fade:
                    musicAudioSource.DOFade(0f, options.FadeDuration).OnComplete(() => {
                        musicAudioSource.Stop();
                        musicAudioSource.clip = null;
                        options.OnComplete?.Invoke();
                    });
                    break;
            }
        }
    }
}
