using CommunityToolkit.Mvvm.Input;
using HamsterStudio.Barefeet.MVVM;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace HamsterStudio.Photogrammetry.ViewModels;

public class PhotogrammetryMainViewModel : KnownViewModel
{
    public ObservableCollection<DepthOfFieldCalculatorViewModel> DepthOfFieldCalculatorViewModels { get; } = [];
    public ICommand AddDepthOfFieldCalculatorViewModelsCommand { get; }

    public ObservableCollection<EquivalentExposureCalculatorViewModel> EquivalentExposureCalculatorViewModels { get; } = [];
    public ICommand AddEquivalentExposureCalculatorViewModelCommand { get; }

    public PhotogrammetryMainViewModel(IServiceProvider provider, ILoggerFactory factory) : base(null)
    {
        DisplayName = "键摄";

        AddDepthOfFieldCalculatorViewModelsCommand = new RelayCommand(() =>
        {
            var viewModel = provider.GetRequiredService<DepthOfFieldCalculatorViewModel>();
            viewModel.DestroyThisViewModelCommand = new RelayCommand(() =>
            {
                DepthOfFieldCalculatorViewModels.Remove(viewModel);
                viewModel.DestroyThisViewModelCommand = null;
            });
            DepthOfFieldCalculatorViewModels.Add(viewModel);
        });
        AddDepthOfFieldCalculatorViewModelsCommand.Execute(null);

        AddEquivalentExposureCalculatorViewModelCommand = new RelayCommand(() =>
        {
            var viewModel = provider.GetRequiredService<EquivalentExposureCalculatorViewModel>();
            viewModel.DestroyThisViewModelCommand = new RelayCommand(() =>
            {
                EquivalentExposureCalculatorViewModels.Remove(viewModel);
                viewModel.DestroyThisViewModelCommand = null;
            });
            EquivalentExposureCalculatorViewModels.Add(viewModel);
        });
        AddEquivalentExposureCalculatorViewModelCommand.Execute(null);
    }

}
