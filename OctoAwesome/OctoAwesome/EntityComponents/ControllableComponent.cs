﻿using engenious;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OctoAwesome.EntityComponents
{
    public sealed class ControllableComponent : EntityComponent
    {
        public bool JumpInput { get; set; }
        public Vector2 MoveInput { get; set; }

        public bool JumpActive { get; set; }
        public int JumpTime { get; set; }
    }
}
