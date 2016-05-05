using Microsoft.Xna.Framework;
using System.Xml.Serialization;
using System.IO;
using System.Linq;
using System;

namespace OctoAwesome.Entities
{
    /// <summary>
    /// Basisklasse für Entitäten, die mit anderen Entitäten und Blöcken kollidieren können.
    /// </summary>
    public class CollidableEntity : Entity, ICollidable
    {
        private readonly float Gap = 0.001f;

        protected IResourceManager ResourceManager { get; private set; }

        /// <summary>
        /// Der Planet, auf dem sich die Entität befindet.
        /// </summary>
        protected IPlanet planet;

        private Index3 _oldIndex;

        /// <summary>
        /// Gibt an, ob die Entität bereit ist.
        /// </summary>
        public bool ReadyState { get; private set; }

        /// <summary>
        /// Der lokale Chunk Cache für die Entität.
        /// </summary>
        protected ILocalChunkCache LocalChunkCache { get; private set; }

        /// <summary>
        /// Die Masse der Entität. 
        /// </summary>
        public float Mass { get; set; }

        /// <summary>
        /// Gibt an, ob der Spieler an Boden ist
        /// </summary>
        [XmlIgnore]
        public bool OnGround { get; set; }

        /// <summary>
        /// Kraft die von aussen auf die Entität wirkt.
        /// </summary>
        [XmlIgnore]
        public Vector3 ExternalForce { get; set; }

        /// <summary>
        /// Der Radius der Entity in Blocks.
        /// </summary>
        public float Radius { get; set; }

        /// <summary>
        /// Die Körperhöhe der Entity in Blocks
        /// </summary>
        public float Height { get; set; }

        /// <summary>
        /// Geschwindikeit der Entität als Vektor
        /// </summary>
        [XmlIgnore]
        public Vector3 Velocity { get; set; }

        public override void Serialize(BinaryWriter writer)
        {
            base.Serialize(writer);

            writer.Write(Mass);
            writer.Write(Radius);
            writer.Write(Height);
        }

        public override void Deserialize(BinaryReader reader)
        {
            base.Deserialize(reader);

            Mass = reader.ReadSingle();
            Radius = reader.ReadSingle();
            Height = reader.ReadSingle();
        }

        public override void Initialize(IResourceManager resourceManager, bool firstTime)
        {
            ResourceManager = resourceManager;
            _oldIndex = Position.ChunkIndex;
            ReadyState = false;
            planet = resourceManager.GetPlanet(Position.Planet);
            LocalChunkCache = new LocalChunkCache(resourceManager.GlobalChunkCache, 2, 1);

            LocalChunkCache.SetCenter(planet, new Index2(Position.ChunkIndex), (success) =>
            {
                ReadyState = success;
                if (firstTime)
                    BeamUp();
            });
        }

        public override void Unload()
        {
            LocalChunkCache.Flush();
        }

        int lastJump = 0;

        public override void Update(GameTime time)
        {
            Velocity += PhysicalUpdate(Vector3.Zero, time.ElapsedGameTime, planet.Gravity, false);

            #region Playerbewegung

            Vector3 move = Velocity * (float)time.ElapsedGameTime.TotalSeconds;

            OnGround = false;

            //Blocks finden die eine Kollision verursachen könnten
            int minx = (int)Math.Floor(Math.Min(
                   Position.BlockPosition.X - Radius,
                   Position.BlockPosition.X - Radius + move.X));
            int maxx = (int)Math.Ceiling(Math.Max(
                Position.BlockPosition.X + Radius,
                Position.BlockPosition.X + Radius + move.X));
            int miny = (int)Math.Floor(Math.Min(
                Position.BlockPosition.Y - Radius,
                Position.BlockPosition.Y - Radius + move.Y));
            int maxy = (int)Math.Ceiling(Math.Max(
                Position.BlockPosition.Y + Radius,
                Position.BlockPosition.Y + Radius + move.Y));
            int minz = (int)Math.Floor(Math.Min(
                Position.BlockPosition.Z,
                Position.BlockPosition.Z + move.Z));
            int maxz = (int)Math.Ceiling(Math.Max(
                Position.BlockPosition.Z + Height,
                Position.BlockPosition.Z + Height + move.Z));

            //Beteiligte Flächen des Spielers
            var playerplanes = CollisionPlane.GetPlayerCollisionPlanes(this).ToList();

            bool abort = false;

            for (int z = minz; z <= maxz && !abort; z++)
            {
                for (int y = miny; y <= maxy && !abort; y++)
                {
                    for (int x = minx; x <= maxx && !abort; x++)
                    {
                        move = Velocity * (float)time.ElapsedGameTime.TotalSeconds;

                        Index3 pos = new Index3(x, y, z);
                        Index3 blockPos = pos + Position.GlobalBlockIndex;
                        ushort block = LocalChunkCache.GetBlock(blockPos);
                        if (block == 0)
                            continue;



                        var blockplane = CollisionPlane.GetBlockCollisionPlanes(pos, Velocity).ToList();

                        var planes = from pp in playerplanes
                                     from bp in blockplane
                                     where CollisionPlane.Intersect(bp, pp)
                                     let distance = CollisionPlane.GetDistance(bp, pp)
                                     where CollisionPlane.CheckDistance(distance, move)
                                     select new { BlockPlane = bp, PlayerPlane = pp, Distance = distance };

                        foreach (var plane in planes)
                        {

                            var subvelocity = (plane.Distance / (float)time.ElapsedGameTime.TotalSeconds);
                            var diff = Velocity - subvelocity;

                            float vx;
                            float vy;
                            float vz;

                            if (plane.BlockPlane.normal.X != 0 && (Velocity.X > 0 && diff.X >= 0 && subvelocity.X >= 0 || Velocity.X < 0 && diff.X <= 0 && subvelocity.X <= 0))
                                vx = subvelocity.X;
                            else
                                vx = Velocity.X;

                            if (plane.BlockPlane.normal.Y != 0 && (Velocity.Y > 0 && diff.Y >= 0 && subvelocity.Y >= 0 || Velocity.Y < 0 && diff.Y <= 0 && subvelocity.Y <= 0))
                                vy = subvelocity.Y;
                            else
                                vy = Velocity.Y;

                            if (plane.BlockPlane.normal.Z != 0 && (Velocity.Z > 0 && diff.Z >= 0 && subvelocity.Z >= 0 || Velocity.Z < 0 && diff.Z <= 0 && subvelocity.Z <= 0))
                                vz = subvelocity.Z;
                            else
                                vz = Velocity.Z;

                            Velocity = new Vector3(vx, vy, vz);

                            if (vx == 0 && vy == 0 && vz == 0)
                            {
                                abort = true;
                                break;
                            }
                        }
                    }
                }
            }

            // TODO: Was ist für den Fall Gravitation = 0 oder im Scheitelpunkt des Sprungs?
            OnGround = Velocity.Z == 0f;

            Coordinate position = Position + Velocity * (float)time.ElapsedGameTime.TotalSeconds;
            position.NormalizeChunkIndexXY(planet.Size);
            Position = position;

            if (Position.ChunkIndex != _oldIndex)
            {
                _oldIndex = Position.ChunkIndex;
                ReadyState = false;
                LocalChunkCache.SetCenter(planet, new Index2(Position.ChunkIndex), (success) =>
                {
                    ReadyState = success;
                });
            }

            #endregion
        }

        /// <summary>
        /// Führt die physikalischen Berechnungen für die Entität durch.
        /// TODO: Hier werden noch konstanten aus dem Player verwendet!
        /// </summary>
        /// <param name="velocitydirection">Die Bewegungsrichtung.</param>
        /// <param name="elapsedtime">Die Zeitspanne seit dem letzten PhysicalUpdate.</param>
        /// <param name="gravity">Gibt an, ob Schwerkraft wirken soll.</param>
        /// <param name="jump">Gibt an, ob ein Sprung durchgeführt werden soll.</param>
        /// <returns></returns>
        protected Vector3 PhysicalUpdate(Vector3 velocitydirection, TimeSpan elapsedtime, Vector3 gravity, bool jump)
        {
            Vector3 exforce = ExternalForce + (gravity * Mass);

            Vector3 externalPower = ((exforce * exforce) / (2 * Mass)) * (float)elapsedtime.TotalSeconds;
            externalPower *= new Vector3(Math.Sign(exforce.X), Math.Sign(exforce.Y), Math.Sign(exforce.Z));

            Vector3 friction = new Vector3(1, 1, 0.1f) * Player.FRICTION;
            Vector3 powerdirection = new Vector3();

            powerdirection += externalPower;
            powerdirection += (Player.POWER * velocitydirection);
            if (jump && OnGround)
            {
                Vector3 jumpDirection = new Vector3(0, 0, 1);
                jumpDirection.Z = 1f;
                jumpDirection.Normalize();
                powerdirection += jumpDirection * Player.JUMPPOWER;
            }

            Vector3 VelocityChange = (2.0f / Mass * (powerdirection - friction * Velocity)) *
                (float)elapsedtime.TotalSeconds;

            return new Vector3(
                (float)(VelocityChange.X < 0 ? -Math.Sqrt(-VelocityChange.X) : Math.Sqrt(VelocityChange.X)),
                (float)(VelocityChange.Y < 0 ? -Math.Sqrt(-VelocityChange.Y) : Math.Sqrt(VelocityChange.Y)),
                (float)(VelocityChange.Z < 0 ? -Math.Sqrt(-VelocityChange.Z) : Math.Sqrt(VelocityChange.Z)));

        }

        /// <summary>
        /// Hebt den Spieler auf Bodenniveau.
        /// </summary>
        public void BeamUp()
        {
            IChunkColumn column = LocalChunkCache.GetChunkColumn(Position.ChunkIndex.X, Position.ChunkIndex.Y);
            int newHeight = column.Heights[Position.LocalBlockIndex.X, Position.LocalBlockIndex.Y] + 1;
            Coordinate newPosition = Position;
            newPosition.GlobalBlockIndex = new Index3(newPosition.GlobalBlockIndex.X,
                newPosition.GlobalBlockIndex.Y,
                newHeight);
            Position = newPosition;
        }

    }
}
