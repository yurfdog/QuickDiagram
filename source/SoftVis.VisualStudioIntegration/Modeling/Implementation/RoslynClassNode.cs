﻿using System.Collections.Generic;
using Codartis.SoftVis.Modeling;
using Microsoft.CodeAnalysis;

namespace Codartis.SoftVis.VisualStudioIntegration.Modeling.Implementation
{
    /// <summary>
    /// A model node created from a Roslyn class symbol.
    /// </summary>
    internal class RoslynClassNode : RoslynTypeNode
    {
        internal RoslynClassNode(ModelNodeId id, INamedTypeSymbol namedTypeSymbol)
            : base(id, namedTypeSymbol, ModelNodeStereotypes.Class)
        {
        }

        public override bool IsAbstract => NamedTypeSymbol.IsAbstract;

        protected override IRoslynModelNode CreateInstance(ModelNodeId id, ISymbol newSymbol)
            => new RoslynClassNode(id, EnsureNamedTypeSymbol(newSymbol));

        public override IEnumerable<RelatedSymbolPair> FindRelatedSymbols(IRoslynModelProvider roslynModelProvider,
            DirectedModelRelationshipType? directedModelRelationshipType = null)
        {
            if (directedModelRelationshipType == null || directedModelRelationshipType == DirectedRelationshipTypes.BaseType)
                foreach (var baseSymbolRelation in GetBaseTypes(NamedTypeSymbol))
                    yield return baseSymbolRelation;

            if (directedModelRelationshipType == null || directedModelRelationshipType == DirectedRelationshipTypes.Subtype)
                foreach (var derivedSymbolRelation in GetDerivedTypes(roslynModelProvider, NamedTypeSymbol))
                    yield return derivedSymbolRelation;

            if (directedModelRelationshipType == null || directedModelRelationshipType == DirectedRelationshipTypes.ImplementedInterface)
                foreach (var implementedSymbolRelation in GetImplementedInterfaces(NamedTypeSymbol))
                    yield return implementedSymbolRelation;
        }

        private static IEnumerable<RelatedSymbolPair> GetBaseTypes(INamedTypeSymbol roslynSymbol)
        {
            var baseSymbol = roslynSymbol.BaseType;
            if (baseSymbol?.TypeKind == TypeKind.Class)
                yield return new RelatedSymbolPair(roslynSymbol, baseSymbol, DirectedRelationshipTypes.BaseType);
        }
    }
}
