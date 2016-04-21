using System;

namespace OctoAwesome
{
    /// <summary>
    /// Interface, un die Ressourcen in OctoAwesome zu verfalten
    /// </summary>
    public interface IResourceManager
    {
        /// <summary>
        /// Lädt das Universum für die angegebene GUID.
        /// </summary>
        /// <param name="universeId">Die Guid des Universums.</param>
        void LoadUniverse(Guid universeId);

        /// <summary>
        /// Entlädt das aktuelle Universum.
        /// </summary>
        void UnloadUniverse();

        /// <summary>
        /// Löscht ein Universum.
        /// </summary>
        /// <param name="id">Die Guid des Universums.</param>
        void DeleteUniverse(Guid id);

        /// <summary>
        /// Lädt einen Player.
        /// </summary>
        /// <param name="playername">Der Name des Players.</param>
        /// <returns></returns>
        Player LoadPlayer(string playername);

        /// <summary>
        /// Speichert einen Player.
        /// </summary>
        /// <param name="player">Der Player.</param>
        void SavePlayer(Player player);

        /// <summary>
        /// Loaden von Entitäten
        /// </summary>
        /// <param name="planetId">Index des Planeten.</param>
        /// <param name="columnIndex">Column-Adresse</param>
        /// <returns>Liste der Entitäten</returns>
        Entity[] LoadEntities(int planetId, Index2 columnIndex);

        /// <summary>
        /// Speichern von Entitäten
        /// </summary>
        /// <param name="planetId">Index des Planeten.</param>
        /// <param name="columnIndex">Column-Adresse</param>
        /// <param name="entites">Liste der Entitäten</param>
        void SaveEntities(int planetId, Index2 columnIndex, Entity[] entites);

        /// <summary>
        /// Entlädt das aktuelle Universum
        /// </summary>
        /// <returns>Das gewünschte Universum, falls es existiert</returns>
        IUniverse GetUniverse();
        
        /// <summary>
        /// Gibt den Planeten mit der angegebenen ID zurück
        /// </summary>
        /// <param name="planetId">Die Planteten-ID des gewünschten Planeten</param>
        /// <returns>Der gewünschte Planet, falls er existiert</returns>
        IPlanet GetPlanet(int planetId);

        /// <summary>
        /// Cache der für alle Chunks verwaltet und diese an lokale Caches weiter gibt.
        /// </summary>
        IGlobalChunkCache GlobalChunkCache { get; }

        /// <summary>
        /// Der globale Entity Cache
        /// </summary>
        EntityCache EntityCache { get; }

        /// <summary>
        /// Item- und Block-Definitions.
        /// </summary>
        IDefinitionManager Definitions { get; }

    }
}