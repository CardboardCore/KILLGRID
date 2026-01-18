using Attic.DI;
using UnityEngine;

namespace Attic.Audio
{
    [RequireComponent(typeof(AudioSource))]
    public class WiredDreamsAudioSource : CardboardCoreBehaviour
    {
        private AudioSource audioSource;

        protected override void OnInjected()
        {
            audioSource = GetComponent<AudioSource>();
        }

        protected override void OnReleased()
        {

        }

        public void Play(AudioClip audioClip, PlaySFXOptions options)
        {
            if (options.Position.HasValue)
            {
                transform.position = options.Position.Value;
            }
            else if (options.Parent)
            {
                transform.SetParent(options.Parent);
                transform.position = options.Parent.position;
            }

            audioSource.clip = audioClip;
            audioSource.volume = options.Volume;
            audioSource.pitch = options.Pitch;
            audioSource.spatialize = true;
            audioSource.spatialBlend = 1f;

            audioSource.Play();

            Destroy(gameObject, audioClip.length * (Time.timeScale < 0.009999999776482582 ? 0.01f : Time.timeScale));
        }
    }
}
