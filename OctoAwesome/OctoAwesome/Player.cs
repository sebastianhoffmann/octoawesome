using Microsoft.Xna.Framework;
using OctoAwesome.Entities;
using System.Linq;

namespace OctoAwesome
{
    /// <summary>
    /// Entität, die der menschliche Spieler mittels Eingabegeräte steuern kann.
    /// </summary>
    public sealed class Player : PermanentEntity, IInventory
    {
        /// <summary>
        /// Die Reichweite des Spielers, in der er mit Spielelementen wie <see cref="Block"/> und <see cref="Entity"/> interagieren kann
        /// </summary>
        public const int SELECTIONRANGE = 8;

        /// <summary>
        /// Die Kraft, die der Spieler hat, um sich fortzubewegen
        /// </summary>
        public const float POWER = 600f;

        /// <summary>
        /// Die Kraft, die der Spieler hat, um in die Luft zu springen
        /// </summary>
        public const float JUMPPOWER = 400000f;

        /// <summary>
        /// Die Reibung die der Spieler mit der Umwelt hat
        /// </summary>
        public const float FRICTION = 60f;

        /// <summary>
        /// Bestimmt den Aktivierungsradius eines Spielers.
        /// </summary>
        public const int ACTIVATIONRANGE = 4;

        /// <summary>
        /// Erzeugt eine neue Player-Instanz an der Default-Position.
        /// </summary>
        public Player() : base(ACTIVATIONRANGE)
        {
            Velocity = new Vector3(0, 0, 0);
            Radius = 0.75f;
            Angle = 0f;
            Height = 3.5f;
            Mass = 100;
        }

        /// <summary>
        /// DEBUG METHODE: NICHT FÜR VERWENDUNG IM SPIEL!
        /// </summary>
        public void AllBlocksDebug()
        {
            var blockDefinitions = ResourceManager.Definitions.GetBlockDefinitions();

            foreach (var blockDefinition in blockDefinitions)
            {

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
        }
    }
}
