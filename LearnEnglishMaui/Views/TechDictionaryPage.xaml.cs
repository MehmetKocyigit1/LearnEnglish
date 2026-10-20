// IngilizceProje/MAUIClient/Views/TechDictionaryPage.xaml.cs
using IngilizceProjeMAUI.ViewModels;
using Microsoft.Maui.Controls;

namespace IngilizceProjeMAUI.Views
{
    public partial class TechDictionaryPage : ContentPage
    {
        private readonly TechDictionaryViewModel _viewModel;

        public TechDictionaryPage(TechDictionaryViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
        }

        protected override void OnDisappearing()
        {
            _viewModel.StopPronunciation();
            base.OnDisappearing();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            if (_viewModel.TotalWordsCount == 0)
            {
                await _viewModel.LoadDatasetAsync();
            }
            else
            {
                await _viewModel.SyncUserWordsAsync();
            }
        }
    }
}
