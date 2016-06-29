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
    class Sound
    {
        SoundEffectInstance soundInstance;
        AudioEmitter emitter;
        AudioListener listener;
        bool loop;
        public Sound(SoundEffect sounds)
        {
            soundInstance = sounds.CreateInstance();
        }

        public void Play(Vector3 position)
        {
            AudioListener listener = new AudioListener();
            AudioEmitter emitter = new AudioEmitter();
            Update(position);
            soundInstance.Play();
            
        }

        public void Update(Vector3 position)
        {
            listener.Position = GameplayScreen.camera.Position;
            listener.Forward = GameplayScreen.camera.ViewDirection; //nie wiem czy to coś daje i czy działa
            emitter.Position = position;
            soundInstance.Apply3D(listener, emitter);
        }
    }
}
