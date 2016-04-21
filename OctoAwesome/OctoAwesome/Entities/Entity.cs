using Microsoft.Xna.Framework;
using OctoAwesome.Entities;
using System.IO;
using System.Xml.Serialization;

namespace OctoAwesome
{
    /// <summary>
    /// Basisklasse für alle selbständigen Wesen
    /// </summary>
    public abstract class Entity : IPosition, IRotatable
    {
        private Coordinate position;

        private float angle = 0f;

        /// <summary>
        /// Die Position der Entität
        /// </summary>
        public Coordinate Position
        {
            get { return position; }
            set
            {
                if (position.Planet != value.Planet || 
                    position.ChunkIndex.X != value.ChunkIndex.X || 
                    position.ChunkIndex.Y != value.ChunkIndex.Y)
                {
                    OnColumnChanged(
                        new PlanetIndex2(position.Planet, new Index2(position.ChunkIndex)), 
                        new PlanetIndex2(value.Planet, new Index2(value.ChunkIndex)));
                }
                position = value;
            }
        }

        /// <summary>
        /// Blickwinkel in der horizontalen Achse
        /// </summary>
        public float Angle
        {
            get { return angle; }
            set { angle = MathHelper.WrapAngle(value); }
        }

        /// <summary>
        /// Blickwinkel in der vertikalen Achse
        /// </summary>
        public float Tilt { get; set; }

        public abstract void Initialize(IResourceManager resourceManager, bool firstTime);

        public abstract void Update(GameTime time);

        public abstract void Unload();

        public virtual void Serialize(BinaryWriter writer)
        {
            // Position
            writer.Write(Position.Planet);
            writer.Write(Position.GlobalBlockIndex.X);
            writer.Write(Position.GlobalBlockIndex.Y);
            writer.Write(Position.GlobalBlockIndex.Z);
            writer.Write(Position.BlockPosition.X);
            writer.Write(Position.BlockPosition.Y);
            writer.Write(Position.BlockPosition.Z);

            writer.Write(Angle);
            writer.Write(Tilt);
        }

        public virtual void Deserialize(BinaryReader reader)
        {
            // Position
            Position = new Coordinate(reader.ReadInt32(),
                new Index3(reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32()),
                new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle()));

            Angle = reader.ReadSingle();
            Tilt = reader.ReadSingle();
        }

        protected virtual void OnColumnChanged(PlanetIndex2 oldColumn, PlanetIndex2 newColumn) { }
    }
}
