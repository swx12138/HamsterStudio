﻿using CommunityToolkit.Mvvm.ComponentModel;
using System.ComponentModel;
using HamsterStudio.HandyUtil.PropertyEditors;
using HandyControl.Controls;

namespace HamsterStudio.ImageTool
{
    partial class PreviewImagePropertiesModel : ObservableObject
    {
        [ObservableProperty]
        private int _colums = 3;

        [ObservableProperty]
        private bool _uniform = true;

        [ObservableProperty]
        [property: Editor(typeof(FilenameEditor), typeof(PropertyEditorBase))]
        private string _savingFilename = string.Empty;

        public event EventHandler? NotifyRedraw;

        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Colums) || e.PropertyName == nameof(Uniform))
            {
                NotifyRedraw?.Invoke(this, EventArgs.Empty);
            }
            base.OnPropertyChanged(e);
        }
    }
}
