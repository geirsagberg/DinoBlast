using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;
using BunnyLand.Models;
using BunnyLand.Views;
using BunnyLand.Controllers;
using System.Diagnostics;
using BunnyLand.Models.Weapons;
using System.Collections;
using BunnyLand.Models.Entities;

namespace BunnyLand.Models
{
    public class Scores
    {
        public int Kills;
        public int Deaths;

        public void AddToKills(int i)
        {
            Kills += i;
        }
        public void AddToDeaths(int i)
        {
            Deaths += i;
        }
    }

    /// <summary>
    /// This class implements the model of the game in action
    /// </summary>
    public class GameplayModel : AbstractModel
    {
        public BunnyGame game;

        public int RespawnDelay { get; private set; }
        public int MaxPlayers { get; private set; }
        private Random rand = new Random();
        public GameplayView View { get; set; }
        public Terrain Terrain { get; private set; }
        public Texture2D TerrainTexture { get; set; }
        public int KillLimit { get; private set; }
        private int numberPlanets, numberBlackHoles;
        public bool GameOver { get; set; }

        public Dictionary<Player, Scores> ScoreBoard { get; set; }

        /* Lists */
        public List<Player> PlayerList { get; private set; }
        public List<BloodParticle> BloodParticleList { get; private set; }
        public List<Projectile> ProjectileList { get; private set; }
        public List<BlackHole> BlackHoleList { get; private set; }
        public List<Planet> PlanetList { get; private set; }
        public List<Entity> EntityList { get; private set; }
        public List<PhysicalObject> PhysicalObjectList { get; private set; }
        public List<GravityPoint> GravityPointList { get; private set; }
        public List<Explosion> ExplosionList { get; private set; }

        /* Max number of entities in the game at once */
        private int entityLimit = 200;

        public bool IsEntityLimitReached { get { return EntityList.Count >= entityLimit; } }

        private CollisionEngine collisionEngine;
        private UpdateEngine updateEngine;

        // The background image for the gameplay scene
        public Texture2D BackGround { get; private set; }


        public GameplayModel(BunnyGame game)
            : base(game)
        {
            GameOver = false;
            this.game = game;
            KillLimit = 1000000;
            PlayerList = new List<Player>();
            BloodParticleList = new List<BloodParticle>();
            ProjectileList = new List<Projectile>();
            BlackHoleList = new List<BlackHole>();
            PlanetList = new List<Planet>();
            EntityList = new List<Entity>();
            PhysicalObjectList = new List<PhysicalObject>();
            GravityPointList = new List<GravityPoint>();
            ExplosionList = new List<Explosion>();
            collisionEngine = new CollisionEngine(this);
            updateEngine = new UpdateEngine(this);
            ScoreBoard = new Dictionary<Player, Scores>();
            MaxPlayers = 4;
            RespawnDelay = 120;
        }

        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();
        }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            updateEngine.UpdateWorld();
            collisionEngine.CheckCollisions();

            base.Update(gameTime);
        }

        /// <summary>
        /// Create a new game, using the provided configuration parameters
        /// </summary>
        /// <param name="mapType">The map type</param>
        /// <param name="mapSize">Size of the map</param>
        /// <param name="numberOfPlayers">Number of players</param>
        /// <param name="killLimit">Number of kills needed for a player to win the game</param>
        /// <param name="numberPlanets">Number of randomly generated planets</param>
        /// <param name="numberBlackHoles">Number of randomly generated black holes</param>
        public void CreateNewGame(MapType mapType, MapSize mapSize, int numberOfPlayers, int killLimit, int numberPlanets, int numberBlackHoles)
        {
            Weapon.GlobalReloadTimeMultiplier = 1;

            GameOver = false;

            KillLimit = killLimit;

            this.numberPlanets = numberPlanets;
            this.numberBlackHoles = numberBlackHoles;

            int[] size = Utility.MapSizeToInts(mapSize);
            Terrain terrain;
            if (mapType == MapType.Simple)
                terrain = new SimpleTerrain(size[0], size[1]);
            else
                terrain = new RandomTerrain(size[0], size[1]);
            NewGame(terrain, rand.Next(2000));

            /* Choose a background image */
            RandomizeBackground();

            /* Add planets */
            for (int i = 0; i < numberPlanets; i++)
            {
                Planet planet = new Planet(0.03f, 0.03f, 0.3f, 1.0f);
                PlaceEntity(planet);
                Add(planet);
            }

            /* Add black holes */
            for (int i = 0; i < numberBlackHoles; i++)
            {
                BlackHole blackHole = new BlackHole(1000, 4000);
                PlaceEntity(blackHole);
                Add(blackHole);
            }

            /* init players */
            InitPlayers(numberOfPlayers);
        }

        /* Start a new game */
        private void NewGame(Terrain terrain, int seed)
        {
            entityLimit = ((int)Settings.EntityLimit + 1) * 100;

            terrain.GenerateTerrain(seed);
            
            //terrain.SetTextureColor(Color.DarkKhaki, Color.DarkGray, 10);
            this.Terrain = terrain;

            TerrainTexture = new Texture2D(Game.GraphicsDevice, terrain.Width, terrain.Height);
            TerrainTexture.SetData<Color>(terrain.GetTextureColor());
            View.InitializeRenderTarget();


            if (Sprites.TerrainBackgrounds != null)
            {
                int i = rand.Next(Sprites.TerrainBackgrounds.Count);
                AddSpriteToTerrain(terrain.Width / 2, terrain.Height / 2, Sprites.TerrainBackgrounds[i], 1f);
            }
            else
            {
                terrain.SetTextureColor(new Color((float)rand.NextDouble(), (float)rand.NextDouble(), (float)rand.NextDouble()), new Color((float)rand.NextDouble(), (float)rand.NextDouble(), (float)rand.NextDouble()), 10);
            }
            terrain.AddGrass(Color.DarkGreen, 20, 5);
            View.DrawToTerrain(terrain.GetTextureColor(), 0, 0, terrain.Width, terrain.Height);

            ClearLists();
        }

        private void InitPlayers(int numberOfplayers)
        {
            List<string> weaponStrings;

            bool writeToFile = false;
            try
            {
                weaponStrings = Utility.ReadFromFile("weapons.ini");
                writeToFile = true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception when loading weapons: " + e + "... defaulting to default weapons!");
                weaponStrings = new List<string>();
                weaponStrings.Add("Pistol|60|1|20|30|10|false|false|10|0|1|0|1|pistol|pistol");
                weaponStrings.Add("Shotgun|120|20|12|10|10|false|false|8|1|10|0.1|1|shotgun|shotgun");
                weaponStrings.Add("SMG|90|1|50|10|10|false|true|10|0|1|0.1|1|smg|MP5");
                weaponStrings.Add("Sniper Rifle|120|20|12|60|15|false|false|20|0|1|0|1|sniperRifle|AG3");
                weaponStrings.Add("Grenade Launcher|120|30|6|150|50|true|true|8|0|1|0|2|grenadeLauncher|SMG");
            }
            for (int i = 0; i < numberOfplayers; i++)
            {
                Player p = new Player();
                p.Health = 200;

                foreach (string s in weaponStrings)
                {
                    p.Weapons.Add(HandgunWeapon.WeaponFromString(s));
                }
                if (!writeToFile)
                {
                    Weapon.WriteWeaponsToIniFile(p.Weapons);
                    writeToFile = true;
                }
                p.CurrentWeapon = p.Weapons.ElementAt(0);
                PlacePlayer(p);
                Add(p);
                ScoreBoard.Add(p, new Scores());
            }
        }

        /// <summary>
        /// Choose a random background that fits the current screen resolution.
        /// </summary>
        private void RandomizeBackground()
        {
            float aspectRatio = Utility.GetAspectRatio();
            Console.WriteLine(aspectRatio);
            Console.WriteLine(16 / 9);

            if (Math.Abs(aspectRatio - ((float)4 / 3)) < 0.01)
            {
                Console.WriteLine(Math.Abs(aspectRatio - (4 / 3)));
                int index = rand.Next(Sprites.BackGrounds_4_3.Count);
                BackGround = Sprites.BackGrounds_4_3.ElementAt(index);
            }
            else if (Math.Abs(aspectRatio - ((float)5 / 4)) < 0.01)
            {
                Console.WriteLine(Math.Abs(aspectRatio - (5 / 4)));
                int index = rand.Next(Sprites.BackGrounds_5_4.Count);
                BackGround = Sprites.BackGrounds_5_4.ElementAt(index);
            }
            else if (Math.Abs(aspectRatio - ((float)16 / 9)) < 0.01)
            {
                Console.WriteLine(Math.Abs(aspectRatio - (16 / 9)));
                int index = rand.Next(Sprites.BackGrounds_16_9.Count);
                BackGround = Sprites.BackGrounds_16_9.ElementAt(index);
            }
            else if (Math.Abs(aspectRatio - ((float)16 / 10)) < 0.01)
            {
                Console.WriteLine(Math.Abs(aspectRatio - (16 / 10)));
                int index = rand.Next(Sprites.BackGrounds_16_10.Count);
                BackGround = Sprites.BackGrounds_16_10.ElementAt(index);
            }
            else
                throw new Exception();
        }

        #region ActionHandling
        public override void ActionStarted(PlayerIndex pi, PlayerAction action)
        {
            Player p = GetPlayer(pi);
            if (p == null)
                return;
            switch (action)
            {
                case PlayerAction.MoveLeft:
                    if (p.IsMovingRight)
                    {
                        PlayerDig(p);
                    }
                    else
                    {
                        p.IsFacingRight = false;
                    }
                    p.IsMovingLeft = true;
                    break;
                case PlayerAction.MoveRight:
                    if (p.IsMovingLeft)
                    {
                        PlayerDig(p);
                    }
                    else
                    {
                        p.IsFacingRight = true;
                    }
                    p.IsMovingRight = true;
                    break;
                case PlayerAction.AimUp:
                    p.IsAimingUp = true;
                    break;
                case PlayerAction.AimDown:
                    p.IsAimingDown = true;
                    break;
                case PlayerAction.Jump:
                    p.IsJumping = true;
                    break;
                case PlayerAction.Fire:
                    p.IsFiring = true;
                    break;
                case PlayerAction.NextWeapon:
                    if (p.Weapons.Count > 0)
                        p.CurrentWeapon = p.Weapons[(p.Weapons.IndexOf(p.CurrentWeapon) + 1) % p.Weapons.Count];
                    break;
                case PlayerAction.PreviousWeapon:
                    if (p.Weapons.Count > 0)
                    {
                        int i = (p.Weapons.IndexOf(p.CurrentWeapon) - 1);
                        if (i < 0)
                            i = p.Weapons.Count - 1;
                        p.CurrentWeapon = p.Weapons[i];
                    }
                    break;
                case PlayerAction.Cancel:
                    if (GameOver)
                        game.GoToMenu(MenuState.MainMenu);
                    else
                        game.GoToMenu(MenuState.PauseMenu);
                    break;
                case PlayerAction.Dig:
                    PlayerDig(p);
                    break;
                default:
                    break;
            }
        }

        public override void ActionStopped(PlayerIndex pi, PlayerAction action)
        {
            Player p = GetPlayer(pi);
            switch (action)
            {
                case PlayerAction.MoveLeft:
                    p.IsMovingLeft = false;
                    if (p.IsMovingRight)
                        p.IsFacingRight = true;
                    break;
                case PlayerAction.MoveRight:
                    p.IsMovingRight = false;
                    if (p.IsMovingLeft)
                        p.IsFacingRight = false;
                    break;
                case PlayerAction.AimUp:
                    p.IsAimingUp = false;
                    break;
                case PlayerAction.AimDown:
                    p.IsAimingDown = false;
                    break;
                case PlayerAction.Fire:
                    p.IsFiring = false;
                    break;
                case PlayerAction.Jump:
                    p.IsJumping = false;
                    break;
                default:
                    break;
            }
        }

        public override void ActionContinued(PlayerIndex pi, PlayerAction action)
        {

        }
        #endregion
        #region Misc

        private void PlayerDig(Player p)
        {
            Vector2 digPos = p.BodyCenter + Utility.ToVector(p.AimingAngle, p.Size.X / 8);
            DestroyCircle((int)digPos.X, (int)digPos.Y, 20);
        }
        Player GetPlayer(PlayerIndex pi)
        {
            Player result = null;
            foreach (Player p in PlayerList)
            {
                if (p.PlayerIndex == pi)
                    result = p;
            }
            return result;
        }
        /// <summary>
        /// Places an entity in the level so its bounding box is fully inside the level and not colliding with the terrain.
        /// </summary>
        /// <param name="e">The entity to be placed.</param>
        public void PlaceEntity(Entity e)
        {
            int timeOutCounter = 100;
            bool freespaceFound = true;
            e.Position = new Vector2(rand.Next(Terrain.Width), rand.Next(Terrain.Height));
            do
            {
                if (CollisionEngine.BoundingBoxTerrainCollision(e.BoundingBox, Terrain) || !Terrain.IsRectangleClear(e.BoundingBox))
                    freespaceFound = false;
                else
                {
                    foreach (Entity e2 in EntityList)
                    {
                        if (e.BoundingBox.Intersects(e2.BoundingBox))
                        {
                            freespaceFound = false;
                        }
                    }
                }
                timeOutCounter--;
                if (timeOutCounter == 0)
                {
                    DestroyCircle((int)e.Position.X, (int)e.Position.Y, (int)(MathHelper.Max(e.Size.X, e.Size.Y) / 2));
                    freespaceFound = true;
                }
                if (!freespaceFound)
                {
                    e.Position -= new Vector2(0, e.Size.Y / 2);
                    if (e.Position.Y - e.Origin.Y < 0)
                        e.Position = new Vector2(rand.Next(Terrain.Width), rand.Next(Terrain.Height));
                }
            } while (!freespaceFound);
        }

        /// <summary>
        /// Places the player randomly on the battlefield. 
        /// If the player is inside the terrain, it is moved up until it is clear.
        /// If no clear spot can be found, try a new random spot 
        /// </summary>
        /// <param name="p">The player.</param>
        public void PlacePlayer(Player p)
        {
            p.Velocity = Vector2.Zero;
            p.Acceleration = Vector2.Zero;
            Vector2 pos = new Vector2(rand.Next(Terrain.Width), rand.Next(Terrain.Height));
            if (Terrain.IsPositionClear(pos + p.Origin))
            {
                p.Position = pos;
                p.IsStandingOn = GroundTypes.Air;
            }
            else
            {
                int counter = 0;
                do
                {
                    pos.Y = (pos.Y - 1) % Terrain.Height;
                    counter++;
                    if (counter >= Terrain.Height)
                    {
                        pos = new Vector2(rand.Next(Terrain.Width), rand.Next(Terrain.Height));
                        counter = 0;
                    }
                } while (!Terrain.IsPositionClear(pos + p.Origin));
                p.Position = pos;
                p.IsStandingOn = GroundTypes.Ground;
            }
        }
        public void PlayerFiredWeapon(Player p)
        {
            if (p.CurrentWeapon != null && !p.CurrentWeapon.IsReloading)
            {
                Audio.FireWeapon(p, View.Camera.Position);

                foreach (Projectile pro in p.CurrentWeapon.Fire(p.AimingAngle, p))
                {
                    Add(pro);
                    //while (CollisionEngine.IntersectPixels(pro.WorldTransform, (int)pro.Size.X, (int)pro.Size.Y, pro.ColorData, p.WorldTransform, (int)p.Size.X, (int)p.Size.Y, p.ColorData)) //dont shoot yourself
                    while (p.BoundingBox.Contains(pro.BoundingBox))
                    {
                        pro.Position += pro.TotalVelocity;
                    }
                    //pro.Position += Vector2.Normalize(pro.TotalVelocity);
                }
                p.CurrentWeapon.CurrentAmmo--;
                if (p.CurrentWeapon.CurrentAmmo <= 0)
                {
                    Audio.WeaponReload(p, View.Camera.Position);
                    p.CurrentWeapon.IsReloading = true;
                    p.CurrentWeapon.ReloadTimeCounter = p.CurrentWeapon.ReloadTime;
                }
            }
            else
            {
                Audio.WeaponEmpty(p, View.Camera.Position);
            }
        }
        public void AddSpriteToTerrain(int centerX, int centerY, Texture2D texture, float scale)
        {
            Color[] colorData = new Color[texture.Width * texture.Height];
            texture.GetData<Color>(colorData);
            int width = texture.Width;
            int height = texture.Height;
            if (scale != 1)
            { //resize sprite to draw to terrain
                width = (int)(texture.Width * scale);
                height = (int)(texture.Height * scale);
                if (width < 1 || height < 1)
                {
                    scale = 1.0f / MathHelper.Min(texture.Width, texture.Height);
                    width = (int)(texture.Width * scale);
                    height = (int)(texture.Height * scale);
                }
                Color[] newColorData = new Color[width * height];
                int i1, i2;
                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        i1 = y * width + x;
                        i2 = (int)(y * width / scale) + (int)(x / scale);
                        newColorData[y * width + x] = colorData[i2];
                    }
                }
                colorData = newColorData;
            }
            View.DrawToTerrain(Terrain.AddToTerrain(centerX, centerY, width, height, false, false, false, colorData), centerX - width / 2, centerY - height / 2, width, height);
        }
        public void DestroyCircle(int centerX, int centerY, int radius)
        {
            View.DrawToTerrain(Terrain.DestroyCircle(centerX, centerY, radius), centerX - radius, centerY - radius, radius * 2 + 1, radius * 2 + 1);
        }
        /* Resets all entity lists to empty */
        private void ClearLists()
        {
            PlayerList.Clear();
            PlanetList.Clear();
            ProjectileList.Clear();
            BlackHoleList.Clear();
            BloodParticleList.Clear();

            EntityList.Clear();
            PhysicalObjectList.Clear();
            GravityPointList.Clear();

        }
        /* These methods add an item to all relevant lists */
        public void Add(Player item)
        {
            Debug.Assert(item != null, "item is null.");
            if (PlayerList.Count <= MaxPlayers)
            {
                item.PlayerIndex = (PlayerIndex)PlayerList.Count;
                PlayerList.Add(item);
                EntityList.Add(item);
                PhysicalObjectList.Add(item);
            }
        }
        public void Add(Projectile item)
        {
            Debug.Assert(item != null, "item is null.");
            ProjectileList.Add(item);
            EntityList.Add(item);
            PhysicalObjectList.Add(item);
        }
        public void Add(BloodParticle item)
        {
            Debug.Assert(item != null, "item is null.");
            BloodParticleList.Add(item);
            EntityList.Add(item);
            PhysicalObjectList.Add(item);
        }
        public void Add(BlackHole item)
        {
            Debug.Assert(item != null, "item is null.");
            BlackHoleList.Add(item);
            EntityList.Add(item);
            GravityPointList.Add(item);
        }

        public void Add(Explosion e)
        {
            Debug.Assert(e != null, "item is null.");
            ExplosionList.Add(e);
            EntityList.Add(e);
            GravityPointList.Add(e);
        }
        public void Add(Planet item)
        {
            Debug.Assert(item != null, "item is null.");
            /*
            Color[] colorData = new Color[item.SourceRect.Width * item.SourceRect.Height];
            item.Spritesheet.GetData<Color>(0, item.SourceRect, colorData, 0, colorData.Length);
            Terrain.AddToTerrain((int)item.Position.X, (int)item.Position.Y, (int)item.Size.X, (int)item.Size.Y, false, true, true, colorData);
             */
            PlanetList.Add(item);
            EntityList.Add(item);
            GravityPointList.Add(item);
        }

        public void Remove(Projectile proj)
        {
            Debug.Assert(proj != null, "Projectile is null.");
            ProjectileList.Remove(proj);
            EntityList.Remove(proj);
            PhysicalObjectList.Remove(proj);
        }

        public void Remove(BloodParticle bp)
        {
            Debug.Assert(bp != null, "Particle is null.");
            BloodParticleList.Remove(bp);
            EntityList.Remove(bp);
            PhysicalObjectList.Remove(bp);
        }


        public void Remove(Explosion e)
        {
            Debug.Assert(e != null, "Explosion is null");
            ExplosionList.Remove(e);
            EntityList.Remove(e);
            GravityPointList.Remove(e);
        }

        #endregion
        #region Collisions
        public void PlayerHit(Projectile proj, Player p, Vector2 collisionPoint)
        {
            if (BunnyGame.DebugMode)
                Console.WriteLine("Player hit!");

            float bloodSpread = MathHelper.PiOver4 / 2; // 22.5 degrees
            float exitSpeed = proj.TotalVelocity.Length() / 3;
            float speedVariance = proj.TotalVelocity.Length() / 10;
            //float ambientBloodSpeed = 3;
            int bloodParticles = (int)proj.Damage;

            Remove(proj);
            if (proj.Ammunition.IsExplosive)
            {
                CreateExplosion(collisionPoint, proj, true);
            }
            else
            {
                p.Health -= proj.Damage;
            }
            //TODO: check if player is killed

            CreateBlood((int)proj.Damage, p.Position, (int)exitSpeed, Utility.ToAngle(proj.Velocity), bloodSpread);

            CreateBlood((int)proj.Damage, p.Position, (int)exitSpeed, 0, MathHelper.TwoPi);

            if (p.Health <= 0) PlayerKilled(p, proj.Owner);
            else Audio.DamagePlayer(p, proj.Owner.CurrentWeapon, View.Camera.Position);
        }

        public void TerrainHit(Projectile proj, Vector2 collisionPoint)
        {
            //TODO: Check if projectile should bounce or add terrain
            if (proj.Ammunition.IsExplosive)
            {

                CreateExplosion(collisionPoint, proj, true);
            }
            else
            {
                DestroyCircle((int)collisionPoint.X, (int)collisionPoint.Y, (int)proj.BlastRadius);
            }
            Remove(proj);
        }
        public void BlackHoleHit(Projectile proj)
        {
            Remove(proj);
        }
        public void BlackHoleHit(BloodParticle bp)
        {
            Remove(bp);
        }

        public void PlanetHit(Projectile proj)
        {
            Vector2 collisionPoint = proj.Position;
            if (proj.Ammunition.IsExplosive)
            {
                CreateExplosion(collisionPoint, proj, true);
            }
            else
            {
                DestroyCircle((int)collisionPoint.X, (int)collisionPoint.Y, (int)proj.BlastRadius);
            }
            Remove(proj);
        }
        public void ProjectileOutsideLevelBounds(Projectile proj)
        {
            //TODO: Make projectile bounce
            Remove(proj);
        }

        public void PlayerMeetsBlackHole(Player p)
        {
            PlayerKilled(p, p);
        }

        private void PlayerKilled(Player p, Player killedBy)
        {
            Audio.KillPlayer(p, View.Camera.Position);
            CreateBlood(100, p.Position, 10);
            CreateBodyParts(p.Position, 10);
            p.Health = p.DefaultHealth;
            p.StartRespawnTimer(RespawnDelay);
            p.Velocity = p.SelfAcceleration = p.SelfVelocity = p.Acceleration = Vector2.Zero;
            p.Enabled = false;

            ScoreBoard[p].AddToDeaths(1);
            if (killedBy == p)
            {
                ScoreBoard[p].AddToKills(-1);
            }
            else if (killedBy != null)
            {
                ScoreBoard[killedBy].AddToKills(1);
            }

            if (ScoreBoard[killedBy].Kills >= KillLimit)
                GameOver = true;

        }

        #endregion

        private void CreateBlood(int numberOfParticles, Vector2 position, int maxSpeed)
        {
            float exitAngle = 0f;
            float spreadAngle = MathHelper.TwoPi;
            CreateBlood(numberOfParticles, position, maxSpeed, exitAngle, spreadAngle);
        }
        private void CreateBlood(int numberOfParticles, Vector2 position, int maxSpeed, float exitAngle, float spreadAngle)
        {
            for (int i = 0; i < numberOfParticles * (0.5 + (int)Settings.GoreLevel); i++)
            {
                if (!IsEntityLimitReached)
                {
                    BloodParticle bp = new BloodParticle();
                    bp.Position = position;
                    bp.Scale = new Vector2((float)rand.NextDouble());
                    bp.Velocity = Utility.ToVector(MathHelper.WrapAngle(exitAngle + (float)rand.NextDouble() * spreadAngle), (float)rand.NextDouble() * maxSpeed);
                    Add(bp);
                }
            }
        }

        private void CreateBodyParts(Vector2 position, int maxSpeed)
        {
            float exitAngle = 0f;
            float spreadAngle = MathHelper.TwoPi;
            float maxRotation = 0.1f;
            foreach (Texture2D t in Sprites.BodyParts)
            {
                BloodParticle bp = new BloodParticle(t);
                bp.Position = position;
                //bp.Scale = new Vector2((float)rand.NextDouble());
                bp.Velocity = Utility.ToVector(MathHelper.WrapAngle(exitAngle + (float)rand.NextDouble() * spreadAngle), (float)rand.NextDouble() * maxSpeed);
                bp.Rotation = (float)rand.NextDouble() * maxRotation;
                Add(bp);
            }
        }

        private void CreateExplosion(Vector2 position, Projectile proj, bool destroyTerrain)
        {
            float radius = proj.BlastRadius;
            float damage = proj.Damage;
            Audio.PlayExplosion(position, View.Camera.Position);
            foreach (Player p in PlayerList)
            {
                if (p.Enabled)
                {
                    float distance = Vector2.Distance(p.Position, position);
                    if (distance <= radius)
                    {
                        float actualDamage = (1 - (distance / radius)) * damage;
                        p.Health -= actualDamage;
                        Vector2 delta = p.Position - position;
                        CreateBlood((int)actualDamage, p.Position, (int)delta.Length(), Utility.ToAngle(delta), MathHelper.PiOver4 / 2);
                        if (p.Health <= 0)
                            PlayerKilled(p, proj.Owner);
                    }
                }
            }
            Explosion e = new Explosion(-(radius * radius));
            e.Position = position;
            e.Scale = new Vector2(2.5f * radius / e.Size.X);
            Add(e);
            if (destroyTerrain)
                DestroyCircle((int)position.X, (int)position.Y, (int)radius);
        }


        public PlayerIndex GetWinner()
        {
            PlayerIndex pi = PlayerIndex.One;
            int maxKills = int.MinValue;
            foreach(Player p in ScoreBoard.Keys)
            {
                if (ScoreBoard[p].Kills > maxKills)
                {
                    pi = p.PlayerIndex;
                    maxKills = ScoreBoard[p].Kills;
                }
            }
            return pi;
        }
    }
}

