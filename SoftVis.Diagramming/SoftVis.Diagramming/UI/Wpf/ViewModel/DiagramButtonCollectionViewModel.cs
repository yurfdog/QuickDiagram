﻿using System.Collections.ObjectModel;
using System.Linq;
using Codartis.SoftVis.Diagramming;
using Codartis.SoftVis.Diagramming.Graph;
using Codartis.SoftVis.Modeling;
using Codartis.SoftVis.UI.Extensibility;

namespace Codartis.SoftVis.UI.Wpf.ViewModel
{
    /// <summary>
    /// Creates and manages the diagram button viewmodels.
    /// </summary>
    internal class DiagramButtonCollectionViewModel : DiagramViewModelBase
    {
        private readonly DiagramButtonViewModelFactory _diagramButtonViewModelFactory;

        public ObservableCollection<DiagramButtonViewModelBase> DiagramButtonViewModels { get; }

        public DiagramButtonCollectionViewModel(IModel model, Diagram diagram,
            IDiagramBehaviourProvider diagramBehaviourProvider)
            : base(model, diagram)
        {
            _diagramButtonViewModelFactory = new DiagramButtonViewModelFactory(model, diagram,
                diagramBehaviourProvider, DiagramDefaults.ButtonRadius, DiagramDefaults.ButtonOverlapParentBy);

            DiagramButtonViewModels = new ObservableCollection<DiagramButtonViewModelBase>();
            CreateButtons();
        }

        public void AssignButtonsTo(DiagramShapeViewModelBase diagramShapeViewModel)
        {
            foreach (var buttonViewModel in DiagramButtonViewModels)
                buttonViewModel.AssociateWith(diagramShapeViewModel);
        }

        public bool AreButtonsAssignedTo(DiagramShapeViewModelBase diagramShapeViewModel)
        {
            return DiagramButtonViewModels.Any(i => i.AssociatedDiagramShapeViewModel == diagramShapeViewModel);
        }

        public void HideButtons()
        {
            foreach (var buttonViewModel in DiagramButtonViewModels)
                buttonViewModel.Hide();
        }

        private void CreateButtons()
        {
            foreach (var buttonViewModel in _diagramButtonViewModelFactory.CreateButtons())
                DiagramButtonViewModels.Add(buttonViewModel);
        }
    }
}