﻿using System.Collections.Generic;
using Codartis.SoftVis.Geometry;

namespace Codartis.SoftVis.Diagramming.Layout.Incremental
{
    /// <summary>
    /// Provides a read-only view of a positioning vertex layer.
    /// </summary>
    internal interface IReadOnlyPositioningVertexLayer : IEnumerable<PositioningVertexBase>
    {
        int LayerIndex { get; }

        double Top { get; set; }
        double Bottom { get; }
        double Height { get; }
        double CenterY { get; }
        Rect2D Rect { get; }

        int Count { get; }
        PositioningVertexBase this[int i] { get; }
        int IndexOf(PositioningVertexBase vertex);
        PositioningVertexBase GetPrevious(PositioningVertexBase vertex);
        PositioningVertexBase GetNext(PositioningVertexBase vertex);
    }
}
