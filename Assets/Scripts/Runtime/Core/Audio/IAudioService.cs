using UnityEngine;

namespace Core.Services.Audio
{
    public interface IAudioService
    {
        void PlayMusic(string clipId);
        void PlayMusic(string clipId, float volume, bool loop, bool persist);
        void PlaySound(string clipId);
        void PlaySound(string clipId, bool loop);
        void StopMusic();
        void StopAllSounds();
        void StopAll();
        void SetVolume(AudioType audioType, float volume);
        void PauseAll();
        void ResumeAll();
        void ResumeSounds();
        bool IsPlaying(string clipId);
    }
}