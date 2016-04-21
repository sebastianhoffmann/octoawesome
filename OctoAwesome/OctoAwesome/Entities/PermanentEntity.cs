namespace OctoAwesome.Entities
{
    /// <summary>
    /// Entität, die dauerhaft simuliert werden muss (z.B. Spieler)
    /// </summary>
    public class PermanentEntity : ControllableEntity
    {
        /// <summary>
        /// Activierungsradius
        /// </summary>
        public readonly int ActivationRange;

        public PermanentEntity(int activationRange)
        {
            ActivationRange = activationRange;
        }

        public override void Initialize(IResourceManager resourceManager, bool firstTime)
        {
            base.Initialize(resourceManager, firstTime);

            resourceManager.EntityCache.Subscribe(planet, new Index2(Position.ChunkIndex), ActivationRange);
        }

        public override void Unload()
        {
            ResourceManager.EntityCache.Unsubscribe(planet, new Index2(Position.ChunkIndex), ActivationRange);

            base.Unload();
        }

        protected override void OnColumnChanged(PlanetIndex2 oldColumn, PlanetIndex2 newColumn)
        {
            base.OnColumnChanged(oldColumn, newColumn);

            IPlanet oldPlanet = ResourceManager.GetPlanet(oldColumn.Planet);
            IPlanet newPlanet = (oldColumn.Planet != newColumn.Planet) ? ResourceManager.GetPlanet(newColumn.Planet) : oldPlanet;

            ResourceManager.EntityCache.Unsubscribe(oldPlanet, new Index2(oldColumn.ColumnIndex), ActivationRange);
            ResourceManager.EntityCache.Subscribe(newPlanet, new Index2(newColumn.ColumnIndex), ActivationRange);
        }
    }
}
