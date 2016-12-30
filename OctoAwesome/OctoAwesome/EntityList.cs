﻿using OctoAwesome.EntityComponents;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OctoAwesome
{
    public class EntityList : IEntityList
    {
        private List<Entity> entities;
        private IChunkColumn column;


        public EntityList(IChunkColumn column)
        {
            this.entities = new List<Entity>();
            this.column = column;
        }

        public int Count
        {
            get
            {
                return entities.Count;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        public void Add(Entity item)
        {
            entities.Add(item);
        }

        public void Clear()
        {
            entities.Clear();
        }

        public bool Contains(Entity item)
        {
            return entities.Contains(item);
        }

        public void CopyTo(Entity[] array, int arrayIndex)
        {
            entities.CopyTo(array, arrayIndex);
        }

        public IEnumerator<Entity> GetEnumerator()
        {
            return entities.GetEnumerator();
        }

        public bool Remove(Entity item)
        {
            return entities.Remove(item);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return entities.GetEnumerator();
        }

        public IEnumerable<FailEntityChunkArgs> FailChunkEntity()
        {
            foreach (var entity in entities)
            {
                if (!entity.Components.ContainsComponent<PositionComponent>())
                    continue;

                var position = entity.Components.GetComponent<PositionComponent>();
                if (position.Position.ChunkIndex.X != column.Index.X || position.Position.ChunkIndex.Y != column.Index.Y)
                {
                    yield return new FailEntityChunkArgs()
                    {
                        Entity = entity,
                        CurrentChunk = new Index2(column.Index),
                        CurrentPlanet = column.Planet,
                        TargetChunk = new Index2(position.Position.ChunkIndex),
                        TargetPlanet = position.Position.Planet,

                    };
                }
            }
        }
    }
}
