using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;
using BunnyLand.Models.Weapons;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework.Storage;


namespace BunnyLand.Models
{


    public static class Audio
    {
        // Sound effects
        public static SoundEffect sfx_shotgun1 { get; set; }
        public static SoundEffect sfx_explosion1 { get; set; }
        public static SoundEffect sfx_splat1 { get; set; }
        public static SoundEffect sfx_mgun1 { get; set; }
        public static SoundEffect sfx_ouch1 { get; set; }
        public static SoundEffect sfx_reload1 { get; set; }
        public static SoundEffect sfx_empty1 { get; set; }

        private static Boolean playMusic = false;
        private static Boolean positionalAudio = true;

        // Determines how much the volume of soundeffects will decrease depending on distance from
        // the center of the screen.
        private static int density = 100;

        // Determines how often the bunny will say "ouch" when taking damage.
        private static int ouchRate = 5;

        // Background-music for gameplay
        public static List<Song> GameplayMusic { get; set; }

        public static void LoadContent(Microsoft.Xna.Framework.Content.ContentManager Content)
        {
            setMusicVol(50);
            Audio.sfx_shotgun1 = Content.Load<SoundEffect>("Audio/shotgun");
            Audio.sfx_explosion1 = Content.Load<SoundEffect>("Audio/grenade");
            Audio.sfx_splat1 = Content.Load<SoundEffect>("Audio/splat1");
            Audio.sfx_mgun1 = Content.Load<SoundEffect>("Audio/smg");
            Audio.sfx_ouch1 = Content.Load<SoundEffect>("Audio/ouch1");
            Audio.sfx_reload1 = Content.Load<SoundEffect>("Audio/reload");
            Audio.sfx_empty1 = Content.Load<SoundEffect>("Audio/empty");

            // Load all background music files for gameplay
            GameplayMusic = LoadAllSongsFromFolder(Content, "Audio/Music");
        }

        /// <summary>
        /// Play background music
        /// </summary>
        public static void startMusic()
        {
            if (!playMusic)
            {
                // Choose a random background tune to play
                int index = Utility.Random.Next(GameplayMusic.Count);
                Song song = GameplayMusic[index];

                MediaPlayer.IsRepeating = true;
                MediaPlayer.Play(song);
                playMusic = true;
                MediaPlayer.Resume();
            }
        }

        /// <summary>
        /// Stop playing background music
        /// </summary>
        public static void stopMusic()
        {
            if (playMusic)
            {
                MediaPlayer.Pause();
                playMusic = false;
            }

        }

        // int vol: desired volume (range 0-100%)
        public static void setMusicVol(double vol)
        {
            MediaPlayer.Volume = (float)(vol / 100);
        }

        public static void setSFXVol(int vol)
        {
        }

        /// <summary>
        /// Play sound effect of weapon firing

        /// </summary>
        /// <param name="p">The player who fired</param>
        /// <param name="cameraPosition">Position of the camera (used for stereo)</param>
        public static void FireWeapon(Player p, Vector2 cameraPosition)
        {
            if (p.CurrentWeapon.SFX_FireWeapon != null)
            {
                Vector3 ePos = PositionVector(p.Position);
                Vector3 lPos = PositionVector(cameraPosition); // owner position
                SoundEffect se = p.CurrentWeapon.SFX_FireWeapon;
                PlaySound(ePos, lPos, se);
            }
        }

        /// <summary>
        /// Play explosion sound effect
        /// </summary>
        /// <param name="position">Position of the explosion</param>
        /// <param name="cameraPosition">Position of the camera (used for stereo)</param>
        public static void PlayExplosion(Vector2 position, Vector2 cameraPosition)
        {
            Vector3 ePos = PositionVector(position);
            Vector3 lPos = PositionVector(cameraPosition); // owner position
            SoundEffect se = Audio.sfx_explosion1;
            PlaySound(ePos, lPos, se);
        }

        /// <summary>
        /// Plays the sound of a bunny yelping in pain.
        /// </summary>
        /// <param name="p">The player that has been damaged</param>
        /// <param name="w">The weapon that was used to cause the damage</param>
        /// <param name="cameraPosition">Position of the camera (used for stereo)</param>
        public static void DamagePlayer(Player p, Weapon w, Vector2 cameraPosition)
        {

            if (w is HandgunWeapon)
            {
                if (((HandgunWeapon)w).NofProjectiles > 1 || w.IsAutomatic)
                {
                    ouchRate++;
                    if (ouchRate == ((HandgunWeapon)w).NofProjectiles)
                    {
                        ouchRate = 0;
                        Vector3 ePos = PositionVector(p.Position);
                        Vector3 lPos = PositionVector(cameraPosition); // owner position
                        SoundEffect se = Audio.sfx_ouch1;
                        PlaySound(ePos, lPos, se);
                    }
                    else
                    {
                        Random random = new Random();
                        int t = random.Next(4);
                        if (t > 2)
                        {
                            Vector3 ePos = PositionVector(p.Position);
                            Vector3 lPos = PositionVector(cameraPosition); // owner position
                            SoundEffect se = Audio.sfx_ouch1;
                            PlaySound(ePos, lPos, se);
                        }
                    }
                }
                else
                {
                    Random random = new Random();
                    int t = random.Next(4);
                    if (t > 2)
                    {
                        Vector3 ePos = PositionVector(p.Position);
                        Vector3 lPos = PositionVector(cameraPosition); // owner position
                        SoundEffect se = Audio.sfx_ouch1;
                        PlaySound(ePos, lPos, se);
                    }
                }
            }

        }

        /// <summary>
        /// Plays the sound of a player being killed
        /// </summary>
        /// <param name="p">The player that is killed</param>
        /// <param name="cameraPosition">Position of the camera (used for stereo)</param>
        public static void KillPlayer(Player p, Vector2 cameraPosition)
        {
            Vector3 ePos = PositionVector(p.Position);
            Vector3 lPos = PositionVector(cameraPosition); // owner position
            SoundEffect se = Audio.sfx_splat1;
            PlaySound(ePos, lPos, se);
        }

        /// <summary>
        /// Plays the sound of a weapon click (out of ammo)
        /// </summary>
        /// <param name="p">The player trying to fire</param>
        /// <param name="cameraPosition">Position of the camera (used for stereo)</param>
        public static void WeaponEmpty(Player p, Vector2 cameraPosition)
        {
            Vector3 ePos = PositionVector(p.Position);
            Vector3 lPos = PositionVector(cameraPosition); // owner position
            SoundEffect se = Audio.sfx_empty1;
            PlaySound(ePos, lPos, se);
        }

        /// <summary>
        /// Plays the sound of a weapon reloading
        /// </summary>
        /// <param name="p">The player reloading</param>
        /// <param name="cameraPosition">Position of the camera (used for stereo)</param>
        public static void WeaponReload(Player p, Vector2 cameraPosition)
        {
            Vector3 ePos = PositionVector(p.Position);
            Vector3 lPos = PositionVector(cameraPosition); // owner position
            SoundEffect se = Audio.sfx_reload1;
            PlaySound(ePos, lPos, se);
        }

        /// <summary>
        /// Play a sound effect.
        /// </summary>
        /// <param name="emitterPosition">Position of the source of sound</param>
        /// <param name="listenerPosition">The position where the sound is heard</param>
        /// <param name="soundEffect">The sound effect to be played</param>
        private static void PlaySound(Vector3 emitterPosition, Vector3 listenerPosition, SoundEffect soundEffect)
        {
            AudioListener listener = new AudioListener();
            AudioEmitter emitter = new AudioEmitter();
            emitter.Position = emitterPosition;
            listener.Position = listenerPosition;

            SoundEffectInstance sei = soundEffect.CreateInstance();
            sei.Apply3D(listener, emitter);
            sei.Play();

        }

        /// <summary>
        /// Converts a Vector2 into a Vector3 for use in stereo.
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        private static Vector3 PositionVector(Vector2 v)
        {
            if (!positionalAudio) return new Vector3(0f, 0f, 0f);
            return new Vector3(v.X / density, v.Y / density, 0f);
        }

        /// <summary>
        /// Load all the songs located in the folder given as a parameter
        /// </summary>
        /// <param name="Content"></param>
        /// <param name="folder"></param>
        /// <returns></returns>
        private static List<Song> LoadAllSongsFromFolder(Microsoft.Xna.Framework.Content.ContentManager Content, string folder)
        {
            List<Song> songs = new List<Song>();
            string[] files = Directory.GetFiles(Path.Combine(StorageContainer.TitleLocation, Path.Combine(Content.RootDirectory, folder)), "*.xnb");
            foreach (string file in files)
            {
                string assetName = Path.Combine(folder, Path.GetFileNameWithoutExtension(file));
                songs.Add(Content.Load<Song>(assetName));
            }
            return songs;
        }

    }
}