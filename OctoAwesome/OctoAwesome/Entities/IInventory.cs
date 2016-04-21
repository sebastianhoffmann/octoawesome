using System.Collections.Generic;

namespace OctoAwesome.Entities
{
    public interface IInventory
    {
        /// <summary>
        /// Das zur Zeit aktive Werkzeug.
        /// </summary>
        InventorySlot ActiveTool { get; set; }

        /// <summary>
        /// Das Inventar des Spielers.
        /// </summary>
        List<InventorySlot> Inventory { get; }
    }
}
