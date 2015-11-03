﻿using System;
using System.Linq;
using Codartis.SoftVis.Common;
using Codartis.SoftVis.Diagramming.Layout.ActionTracking;
using Codartis.SoftVis.Diagramming.Layout.Incremental.ActionTracking;
using Codartis.SoftVis.Geometry;
using Codartis.SoftVis.Graphs;

namespace Codartis.SoftVis.Diagramming.Layout.Incremental
{
    /// <summary>
    /// Calculates diagram node positions and diagram connector routes when they are added or removed.
    /// </summary>
    internal class DiagramNodePositionCalculator : IncrementalLayoutActionEventSource, IDiagramChangeConsumer
    {
        private readonly IReadOnlyDiagramGraph _diagramGraph;
        private readonly IDiagramNodeRankProvider _diagramNodeRankProvider;

        private readonly PositioningGraph _positioningGraph;
        private readonly Map<DiagramNode, DiagramNodePositioningVertex> _diagramNodeToPositioningVertexMap;
        private readonly Map<DiagramConnector, PositioningEdgePath> _diagramConnectorToPositioningEdgePathMap;
        private readonly Map<PositioningEdgePath, Route> _positioningEdgePathToPreviousRouteMap;

        private readonly VertexPositioningLogic _vertexPositioningLogic;

        public DiagramNodePositionCalculator(double horizontalGap, double verticalGap,
            IReadOnlyDiagramGraph diagramGraph, IDiagramNodeRankProvider diagramNodeRankProvider)
        {
            _diagramGraph = diagramGraph;
            _diagramNodeRankProvider = diagramNodeRankProvider;

            _positioningGraph = new PositioningGraph(horizontalGap, verticalGap);
            _diagramNodeToPositioningVertexMap = new Map<DiagramNode, DiagramNodePositioningVertex>();
            _diagramConnectorToPositioningEdgePathMap = new Map<DiagramConnector, PositioningEdgePath>();
            _positioningEdgePathToPreviousRouteMap = new Map<PositioningEdgePath, Route>();

            _vertexPositioningLogic = new VertexPositioningLogic(horizontalGap, verticalGap, _positioningGraph);
            _vertexPositioningLogic.LayoutActionExecuted += OnLayoutActionExecuted;
        }

        public void Clear()
        {
            _positioningGraph.Clear();
            _diagramNodeToPositioningVertexMap.Clear();
            _diagramConnectorToPositioningEdgePathMap.Clear();
            _positioningEdgePathToPreviousRouteMap.Clear();
        }

        public void Add(DiagramNode diagramNode)
        {
            var layoutAction = RaiseDiagramNodeLayoutAction("AddNode", diagramNode);

            var diagramNodePositioningVertex = new DiagramNodePositioningVertex(_positioningGraph, diagramNode);
            _diagramNodeToPositioningVertexMap.Set(diagramNode, diagramNodePositioningVertex);

            _positioningGraph.AddVertex(diagramNodePositioningVertex);

            _vertexPositioningLogic.PositionVertex(diagramNodePositioningVertex, layoutAction);
            _vertexPositioningLogic.Compact(layoutAction);
        }

        public void Remove(DiagramNode diagramNode)
        {
            var layoutAction = RaiseDiagramNodeLayoutAction("RemoveNode", diagramNode);

            var positioningVertex = _diagramNodeToPositioningVertexMap.Get(diagramNode);

            _vertexPositioningLogic.CoverUpVertex(positioningVertex, layoutAction);

            _positioningGraph.RemoveVertex(positioningVertex);
            _diagramNodeToPositioningVertexMap.Remove(diagramNode);

            _vertexPositioningLogic.Compact(layoutAction);
        }

        public void Add(DiagramConnector diagramConnector)
        {
            var layoutAction = RaiseDiagramConnectorLayoutAction("AddConnector", diagramConnector);

            var positioningSource = _diagramNodeToPositioningVertexMap.Get(diagramConnector.Source);
            var positioningTarget = _diagramNodeToPositioningVertexMap.Get(diagramConnector.Target);
            var newEdge = new PositioningEdge(_positioningGraph, positioningSource, positioningTarget, diagramConnector);
            var newPath = new PositioningEdgePath(newEdge);
            _diagramConnectorToPositioningEdgePathMap.Set(diagramConnector, newPath);
            _positioningGraph.AddPath(newPath);

            AdjustPathsToLayerSpansRecursive(diagramConnector.Source, layoutAction);
            PositionVerticesOnModifiedPathsRecursive(diagramConnector.Source, layoutAction);

            _vertexPositioningLogic.Compact(layoutAction);

            ReroutePath(newPath, layoutAction);
        }

        public void Remove(DiagramConnector diagramConnector)
        {
            var layoutAction = RaiseDiagramConnectorLayoutAction("RemoveConnector", diagramConnector);

            var positioningEdgePath = _diagramConnectorToPositioningEdgePathMap.Get(diagramConnector);

            foreach (var interimVertex in positioningEdgePath.InterimVertices)
                _vertexPositioningLogic.CoverUpVertex(interimVertex, layoutAction);

            // TODO: needs test case: when is this necessary?
            //_vertexPositioningLogic.CenterPrimaryParent(positioningEdgePath.Source, layoutAction);

            _positioningGraph.RemovePath(positioningEdgePath);
            _diagramConnectorToPositioningEdgePathMap.Remove(diagramConnector);

            AdjustPathsToLayerSpansRecursive(diagramConnector.Source, layoutAction);
            // TODO: needs test cases
            PositionVerticesOnModifiedPathsRecursive(diagramConnector.Source, layoutAction);

            _vertexPositioningLogic.Compact(layoutAction);
        }

        private void AdjustPathsToLayerSpansRecursive(DiagramNode updateRootNode, ILayoutAction causingAction)
        {
            _diagramGraph.ExecuteOnVerticesRecursive(updateRootNode, EdgeDirection.In,
                i => AdjustPathsToLayerSpans(i, causingAction));
        }

        private void PositionVerticesOnModifiedPathsRecursive(DiagramNode updateRootNode, ILayoutAction causingAction)
        {
            _diagramGraph.ExecuteOnVerticesRecursive(updateRootNode, EdgeDirection.In,
                i => PositionVerticesOnModifiedPaths(i, causingAction));
        }

        private void AdjustPathsToLayerSpans(DiagramNode diagramNode, ILayoutAction causingAction)
        {
            var outConnectors = _diagramGraph.OutEdges(diagramNode);
            foreach (var outConnector in outConnectors)
            {
                var positioningEdgePath = _diagramConnectorToPositioningEdgePathMap.Get(outConnector);
                var diagramConnectorRankSpan = _diagramNodeRankProvider.GetRankSpan(outConnector);

                var pathLengthDifference = diagramConnectorRankSpan - positioningEdgePath.Length;

                if (pathLengthDifference > 0)
                    SplitEdge(positioningEdgePath, 0, pathLengthDifference, causingAction);
                else if (pathLengthDifference < 0)
                    MergeEdgeWithNext(positioningEdgePath, 0, -pathLengthDifference, causingAction);
            }
        }

        private void PositionVerticesOnModifiedPaths(DiagramNode diagramNode, ILayoutAction causingAction)
        {
            var outConnectors = _diagramGraph.OutEdges(diagramNode);
            foreach (var outConnector in outConnectors)
            {
                var positioningEdgePath = _diagramConnectorToPositioningEdgePathMap.Get(outConnector);
                foreach (var edge in positioningEdgePath.Reverse())
                {
                    _vertexPositioningLogic.PositionVertex(edge.Source, causingAction, edge.Target);
                }
            }
        }

        private void SplitEdge(PositioningEdgePath path, int atIndex, int times, ILayoutAction causingAction)
        {
            for (var i = 0; i < times; i++)
                SplitEdge(path, atIndex, causingAction);
        }

        private void SplitEdge(PositioningEdgePath path, int atIndex, ILayoutAction causingAction)
        {
            var edgeToSplit = path[atIndex];
            var interimVertex = new DummyPositioningVertex(_positioningGraph, true);
            var newEdge1 = new PositioningEdge(_positioningGraph, edgeToSplit.Source, interimVertex, edgeToSplit.DiagramConnector);
            var newEdge2 = new PositioningEdge(_positioningGraph, interimVertex, edgeToSplit.Target, edgeToSplit.DiagramConnector);

            path.Substitute(atIndex, 1, newEdge1, newEdge2);

            _positioningGraph.RemoveEdge(edgeToSplit);
            _positioningGraph.AddVertex(interimVertex);
            _positioningGraph.AddEdge(newEdge1);
            _positioningGraph.AddEdge(newEdge2);

            RaiseVertexLayoutAction("DummyVertexCreated", interimVertex, causingAction);
        }

        private void MergeEdgeWithNext(PositioningEdgePath path, int atIndex, int times, ILayoutAction causingAction)
        {
            for (var i = 0; i < times; i++)
                MergeEdgeWithNext(path, atIndex, causingAction);
        }

        private void MergeEdgeWithNext(PositioningEdgePath path, int atIndex, ILayoutAction causingAction)
        {
            var firstEdge = path[atIndex];
            var nextEdge = path[atIndex + 1];
            var vertexToRemove = firstEdge.Target as DummyPositioningVertex;
            var mergedEdge = new PositioningEdge(_positioningGraph, firstEdge.Source, nextEdge.Target, firstEdge.DiagramConnector);

            if (vertexToRemove == null)
                throw new Exception("FirstEdge.Target is null or not dummy!");

            var layoutAction = RaiseVertexLayoutAction("DummyVertexRemoved", vertexToRemove, causingAction);
            _vertexPositioningLogic.CoverUpVertex(vertexToRemove, layoutAction);

            path.Substitute(atIndex, 2, mergedEdge);

            _positioningGraph.RemoveEdge(firstEdge);
            _positioningGraph.RemoveEdge(nextEdge);
            _positioningGraph.RemoveVertex(vertexToRemove);
            _positioningGraph.AddEdge(mergedEdge);
        }

        private void OnLayoutActionExecuted(object sender, ILayoutAction layoutAction)
        {
            RaiseLayoutAction(sender, layoutAction);

            var vertexMoveAction = layoutAction as IMoveVertexAction;
            if (vertexMoveAction == null)
                return;

            foreach (var edge in vertexMoveAction.Vertex.AllEdges)
            {
                var path = _diagramConnectorToPositioningEdgePathMap.Get(edge.DiagramConnector);
                ReroutePath(path, layoutAction);
            }
        }

        private void ReroutePath(PositioningEdgePath path, ILayoutAction causingAction)
        {
            if (path.IsFloating)
                return;

            var oldRoute = _positioningEdgePathToPreviousRouteMap.Get(path);
            var newRoute = path.GetRoute();
            if (oldRoute == newRoute)
                return;

            _positioningEdgePathToPreviousRouteMap.Set(path, newRoute);
            RaisePathLayoutAction("Reroute", path, oldRoute, newRoute, causingAction);
        }
    }
}
