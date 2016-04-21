using OctoAwesome.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace OctoAwesome.Entities
{
    /// <summary>
    /// Dog Entity
    /// TODO: Move to OctoAwesome.Basics
    /// </summary>
    public class Dog : ControllableEntity
    {
        public Dog()
        {
            Radius = 0.5f;
            Height = 1f;
            Mass = 100;
        }
    }
}
