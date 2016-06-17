﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Codartis.SoftVis.Modeling;
using Codartis.SoftVis.UI.Common;
using Codartis.SoftVis.UI.Wpf.Commands;

namespace Codartis.SoftVis.UI.Wpf.ViewModel
{
    /// <summary>
    /// View model for selecting a model entity.
    /// </summary>
    public class ModelEntitySelectorViewModel : ViewModelBase
    {
        private bool _isVisible;
        private double _width;
        private double _height;
        private double _top;
        private double _left;
        private HandleOrientation _handleOrientation;
        private List<IModelEntity> _modelEntities;
        private IModelEntity _selectedModelEntity;

        public ModelEntityDelegateCommand ModelEntitySelectedCommand { get; }
        public event Action<IModelEntity> ModelEntitySelected;

        public ModelEntitySelectorViewModel(Size size)
        {
            _width = size.Width;
            _height = size.Height;

            ModelEntitySelectedCommand = new ModelEntityDelegateCommand(i => ModelEntitySelected?.Invoke(i));
        }

        public void Show(Point attachPoint, HandleOrientation handleOrientation, IEnumerable<IModelEntity> modelEntities)
        {
            IsVisible = true;
            HandleOrientation = handleOrientation;
            Top = CalculateTop(attachPoint, handleOrientation);
            Left = CalculateLeft(attachPoint, handleOrientation);
            ModelEntities = modelEntities.ToList();
            SelectedModelEntity = null;
        }

        public void Hide()
        {
            IsVisible = false;
        }

        public bool IsVisible
        {
            get { return _isVisible; }
            set
            {
                _isVisible = value;
                OnPropertyChanged();
            }
        }

        public double Width
        {
            get { return _width; }
            set
            {
                _width = value;
                OnPropertyChanged();
            }
        }

        public double Height
        {
            get { return _height; }
            set
            {
                _height = value;
                OnPropertyChanged();
            }
        }

        public double Top
        {
            get { return _top; }
            set
            {
                _top = value;
                OnPropertyChanged();
            }
        }

        public double Left
        {
            get { return _left; }
            set
            {
                _left = value;
                OnPropertyChanged();
            }
        }

        public HandleOrientation HandleOrientation
        {
            get { return _handleOrientation; }
            set
            {
                _handleOrientation = value;
                OnPropertyChanged();
            }
        }

        public List<IModelEntity> ModelEntities
        {
            get { return _modelEntities; }
            set
            {
                _modelEntities = value;
                OnPropertyChanged();
            }
        }

        public IModelEntity SelectedModelEntity
        {
            get { return _selectedModelEntity; }
            set
            {
                _selectedModelEntity = value;
                OnPropertyChanged();
            }
        }

        private double CalculateTop(Point attachPoint, HandleOrientation handleOrientation)
        {
            switch (handleOrientation)
            {
                case HandleOrientation.Top:
                    return attachPoint.Y;
                case HandleOrientation.Bottom:
                    return attachPoint.Y - Height;

                default: throw new NotImplementedException();
            }
        }

        private double CalculateLeft(Point attachPoint, HandleOrientation handleOrientation)
        {
            switch (handleOrientation)
            {
                case HandleOrientation.Top:
                case HandleOrientation.Bottom:
                    return attachPoint.X - Width / 2;

                default: throw new NotImplementedException();
            }
        }
    }
}