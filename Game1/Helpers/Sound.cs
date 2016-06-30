using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Audio;
using Game1.Screens;

namespace Game1.Helpers
{
    public class Sound
    {
        public SoundEffectInstance soundInstance;
        AudioEmitter emitter;
        AudioListener listener;
        public Sound(SoundEffect sounds)
        {
            soundInstance = sounds.CreateInstance();
            soundInstance.Volume = 1.0f;
            listener = new AudioListener();
            emitter = new AudioEmitter();
        }

        public void Play(Vector3 position)
        {
            Update(position);
            soundInstance.Play();
        }

        public void PlaySimple()
        {
            soundInstance.Play();
        }

        public void Stop()
        {
            soundInstance.Stop();
        }

        public void Update(Vector3 position)
        {
            listener.Position = GameplayScreen.camera.Position;
            listener.Forward = -GameplayScreen.camera.ViewDirection; //nie wiem czy to coś daje i czy działa
            emitter.Position = position;
            soundInstance.Apply3D(listener, emitter);
        }
    }
}
