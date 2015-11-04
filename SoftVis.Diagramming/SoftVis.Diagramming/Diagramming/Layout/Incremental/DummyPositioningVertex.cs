﻿using System;
using Codartis.SoftVis.Geometry;

namespace Codartis.SoftVis.Diagramming.Layout.Incremental
{
    /// <summary>
    /// Dummy vertices are used to break long edges spanning more than two layers.
    /// They ensure that adjacent vertices in the positioning graph are always on adjacent layers.
    /// </summary>
    internal class DummyPositioningVertex : PositioningVertexBase
    {
        private static int _nextId = 1;
        protected int Id;

        public DummyPositioningVertex(bool isFloating)
            :base(isFloating)
        {
            lock (typeof (DummyPositioningVertex))
            {
                Id = _nextId;
                _nextId++;
            }
        }

        public override bool IsDummy => true;
        public override string Name => ToString();

        public override int Priority
        {
            get { throw new InvalidOperationException("Dummy vertex has no priority."); }
        }

        public override Size2D Size => Size2D.Zero;

        public override string ToString()
        {
            return $"Dummy#{Id}";
        }
    }
}
