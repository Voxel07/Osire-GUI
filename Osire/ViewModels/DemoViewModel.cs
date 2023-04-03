using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Osire.ViewModels
{
    public partial class DemoViewModel : ObservableObject
    {
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(GetCounter))]
        int number;

        public string GetCounter
        {
            get
            {
                return Number switch
                {
                    0 => "Click me !",
                    1 => "Clicked 1 time",
                    int count => $"Clicked {count} times"
                };
            }
        }

        [RelayCommand]
        private void IncNum()
        {
            Number++;
        }
        [RelayCommand]
        private void DecNum()
        {
            Number--;
        }
    }
}
