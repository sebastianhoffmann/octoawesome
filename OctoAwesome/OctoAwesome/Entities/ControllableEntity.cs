using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace OctoAwesome.Entities
{
    /// <summary>
    /// Basisklasse für alle steuerbaren Entitäten.
    /// </summary>
    public class ControllableEntity : CollidableEntity, IControllable
    {
        private bool lastJump = false;

        private Index3? lastInteract = null;
        private Index3? lastApply = null;
        private OrientationFlags lastOrientation = OrientationFlags.None;

        /// <summary>
        /// Bewegungsvektor des Spielers.
        /// </summary>
        public Vector2 Move { get; set; }

        /// <summary>
        /// Kopfbewegeungsvektor des Spielers.
        /// </summary>
        public Vector2 Head { get; set; }

        /// <summary>
        /// Gibt an, ob der Flugmodus aktiviert ist.
        /// </summary>
        public bool FlyMode { get; set; }

        /// <summary>
        /// Den Spieler hüpfen lassen.
        /// </summary>
        public void Jump()
        {
            lastJump = true;
        }

        /// <summary>
        /// Lässt den Spieler einen Block entfernen.
        /// </summary>
        /// <param name="blockIndex"></param>
        public void Interact(Index3 blockIndex)
        {
            lastInteract = blockIndex;
        }

        /// <summary>
        /// Setzt einen neuen Block.
        /// </summary>
        /// <param name="blockIndex"></param>
        /// <param name="orientation"></param>
        public void Apply(Index3 blockIndex, OrientationFlags orientation)
        {
            lastApply = blockIndex;
            lastOrientation = orientation;
        }

        /// <summary>
        /// Das zur Zeit aktive Werkzeug.
        /// </summary>
        public InventorySlot ActiveTool { get; set; }

        public List<InventorySlot> Inventory { get; set; }

        public ControllableEntity()
        {
            Inventory = new List<InventorySlot>();
            ActiveTool = null;
            FlyMode = false;
        }


        public override void Initialize(IResourceManager resourceManager, bool firstTime)
        {
            base.Initialize(resourceManager, firstTime);
        }

        public override void Unload()
        {
            base.Unload();
        }

        public override void Update(GameTime time)
        {
            #region Inputverarbeitung

            // Input verarbeiten
            Angle += (float)time.ElapsedGameTime.TotalSeconds * Head.X;
            Tilt += (float)time.ElapsedGameTime.TotalSeconds * Head.Y;
            Tilt = Math.Min(1.5f, Math.Max(-1.5f, Tilt));

            #endregion

            #region Physik

            float lookX = (float)Math.Cos(Angle);
            float lookY = -(float)Math.Sin(Angle);
            var velocitydirection = new Vector3(lookX, lookY, 0) * Move.Y;

            float stafeX = (float)Math.Cos(Angle + MathHelper.PiOver2);
            float stafeY = -(float)Math.Sin(Angle + MathHelper.PiOver2);
            velocitydirection += new Vector3(stafeX, stafeY, 0) * Move.X;

            Vector3 gravity = planet.Gravity;
            if (FlyMode)
            {
                velocitydirection += new Vector3(0, 0, (float)Math.Sin(Tilt) * Move.Y);
                gravity = Vector3.Zero;
                // friction = Vector3.One * Player.FRICTION;
            }

            Velocity += PhysicalUpdate(velocitydirection, time.ElapsedGameTime, gravity, lastJump);
            lastJump = false;

            #endregion

            #region Block Interaction

            if (lastInteract.HasValue)
            {
                ushort lastBlock = LocalChunkCache.GetBlock(lastInteract.Value);
                LocalChunkCache.SetBlock(lastInteract.Value, 0);

                if (lastBlock != 0)
                {
                    var blockDefinition = ResourceManager.Definitions.GetBlockDefinitionByIndex(lastBlock);

                    var slot = Inventory.Where(s => s.Definition == blockDefinition && s.Amount < blockDefinition.StackLimit).FirstOrDefault();

                    // Wenn noch kein Slot da ist oder der vorhandene voll, dann neuen Slot
                    if (slot == null || slot.Amount >= blockDefinition.StackLimit)
                    {
                        slot = new InventorySlot()
                        {
                            Definition = blockDefinition,
                            Amount = 0
                        };
                        Inventory.Add(slot);
                    }
                    slot.Amount++;
                }
                lastInteract = null;
            }

            if (lastApply.HasValue)
            {
                if (ActiveTool != null)
                {
                    Index3 add = new Index3();
                    switch (lastOrientation)
                    {
                        case OrientationFlags.SideWest: add = new Index3(-1, 0, 0); break;
                        case OrientationFlags.SideEast: add = new Index3(1, 0, 0); break;
                        case OrientationFlags.SideSouth: add = new Index3(0, -1, 0); break;
                        case OrientationFlags.SideNorth: add = new Index3(0, 1, 0); break;
                        case OrientationFlags.SideBottom: add = new Index3(0, 0, -1); break;
                        case OrientationFlags.SideTop: add = new Index3(0, 0, 1); break;
                    }

                    if (ActiveTool.Definition is IBlockDefinition)
                    {
                        IBlockDefinition definition = ActiveTool.Definition as IBlockDefinition;
                        LocalChunkCache.SetBlock(lastApply.Value + add, ResourceManager.Definitions.GetBlockDefinitionIndex(definition));

                        ActiveTool.Amount--;
                        if (ActiveTool.Amount <= 0)
                        {
                            Inventory.Remove(ActiveTool);
                            ActiveTool = null;
                        }
                    }

                    // TODO: Fix Interaction ;)
                    //ushort block = _manager.GetBlock(lastApply.Value);
                    //IBlockDefinition blockDefinition = BlockDefinitionManager.GetForType(block);
                    //IItemDefinition itemDefinition = ActiveTool.Definition;

                    //blockDefinition.Hit(blockDefinition, itemDefinition.GetProperties(null));
                    //itemDefinition.Hit(null, blockDefinition.GetProperties(block));
                }

                lastApply = null;
            }

            #endregion

            base.Update(time);
        }
    }
}
