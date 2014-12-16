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
        private Dictionary<string, Dictionary<string, Cue>> m_audioQueue;
        private Vector3 listenerPosition;
        private Vector3 listenerVelocity;
        private Vector3 listenerUp;
        private Vector3 listenerForward;

        public static AudioManager Instance
        {
            // lazy instantiate an instance
            get { return m_instance; }
            set { m_instance = value; }
        }

        public AudioManager(Game game) : base(game)
        {
            m_audioEngine = new AudioEngine("Content\\FinalProjectAudio.xgs");
            m_waveBank = new WaveBank(m_audioEngine, "Content\\Wave Bank.xwb");
            m_soundBank = new SoundBank(m_audioEngine, "Content\\Sound Bank.xsb");
            m_instance = this;
            m_audioQueue = new Dictionary<string, Dictionary<string, Cue>>();
            MediaPlayer.IsRepeating = true;
        }

        public override void Update(GameTime gameTime)
        {
            m_audioEngine.Update();
        }

        public void UpdatePosition(Vector3 p, Vector3 v, Vector3 u, Vector3 f)
        {
            listenerPosition = p;
            listenerVelocity = v;
            listenerUp = u;
            listenerForward = f;
        }

        public void Play(string name, string player, Vector3 emit, Vector3 emitV, Vector3 emitU, Vector3 emitF)
        {
            // check if the audio is already in the queue
            var queue = m_audioQueue[player];
            if (queue.ContainsKey(name))
            {
                // if it is, check the various conditions it might be experiencing and act appropriately
                var cue = queue[name];
                if (cue.IsPlaying)
                    return;

                if (cue.IsDisposed || cue.IsStopped || cue.IsStopping)
                    cue = m_soundBank.GetCue(name);
                if (cue.IsPaused)
                    cue.Resume();
                if (!cue.IsPlaying)
                {
                    AudioEmitter emitter = new AudioEmitter();
                    emitter.Position = emit;
                    emitter.Velocity = emitV;
                    emitter.Up = emitU;
                    emitter.Forward = emitF;

                    AudioListener listener = new AudioListener();
                    listener.Position = listenerPosition;
                    listener.Velocity = listenerVelocity;
                    listener.Up = listenerUp;
                    listener.Forward = listenerForward;

                    cue.Apply3D(listener, emitter);
                    cue.Play();
                }
            }
            else
            {
                // otherwise, start a new instance
                queue.Add(name, m_soundBank.GetCue(name));
                queue[name].Play();
            }
        }

        public void AddPlayer(string name)
        {
            if(!m_audioQueue.ContainsKey(name))
                m_audioQueue.Add(name, new Dictionary<string, Cue>());
        }

        public void Pause(string name, string player)
        {
            if (m_audioQueue[player].ContainsKey(name))
            {
                var cue = m_audioQueue[player][name];
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

        public void Clear(string player)
        {
            foreach (var audio in m_audioQueue[player])
                if(!audio.Value.IsDisposed)
                    audio.Value.Stop(AudioStopOptions.Immediate);
            m_audioQueue.Clear();
        }
    }
}
