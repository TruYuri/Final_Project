using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace FinalProject
{
    // AudioManager, singleton
    // Manages the audio playback throughout the game

    class AudioManager : Microsoft.Xna.Framework.GameComponent
    {
        private static AudioManager m_instance;
        private AudioEngine m_audioEngine;
        private WaveBank m_waveBank;
        private SoundBank m_soundBank;
        private Dictionary<string, Cue> m_audioQueue;

        public static AudioManager Instance
        {
            // lazy instantiate an instance
            get { return m_instance; }
            set { m_instance = value; }
        }

        public AudioManager(Game game) : base(game)
        {
            m_audioEngine = new AudioEngine(@"Content\Audio\Lunar_Lander_Audio.xgs");
            m_waveBank = new WaveBank(m_audioEngine, @"Content\Audio\Wave Bank.xwb");
            m_soundBank = new SoundBank(m_audioEngine, @"Content\Audio\Sound Bank.xsb");

            m_audioQueue = new Dictionary<string, Cue>();
            MediaPlayer.IsRepeating = true;
        }

        public void Update(GameTime gameTime)
        {
            m_audioEngine.Update();
        }

        public void Play(string name)
        {
            // check if the audio is already in the queue
            if (m_audioQueue.ContainsKey(name))
            {
                // if it is, check the various conditions it might be experiencing and act appropriately
                var cue = m_audioQueue[name];
                if (cue.IsDisposed || cue.IsStopped || cue.IsStopping)
                    cue = m_soundBank.GetCue(name);
                if (cue.IsPaused)
                    cue.Resume();
                if(!cue.IsPlaying)
                    cue.Play();
            }
            else
            {
                // otherwise, start a new instance
                m_audioQueue.Add(name, m_soundBank.GetCue(name));
                m_audioQueue[name].Play();
            }
        }

        public void Pause(string name)
        {
            if (m_audioQueue.ContainsKey(name))
            {
                var cue = m_audioQueue[name];
                if (!cue.IsDisposed && !cue.IsStopped && !cue.IsStopping)
                    cue.Pause();
            }
        }

        public void PlaySong(Song song)
        {
            MediaPlayer.Play(song);
        }

        public void StopSong()
        {
            MediaPlayer.Stop();
        }

        public void Clear()
        {
            foreach (var audio in m_audioQueue)
                if(!audio.Value.IsDisposed)
                    audio.Value.Stop(AudioStopOptions.Immediate);
            m_audioQueue.Clear();
        }
    }
}
