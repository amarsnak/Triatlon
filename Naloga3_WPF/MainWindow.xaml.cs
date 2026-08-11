// MainWindow.xaml.cs
using System.Windows;
using System.Windows.Controls;
using Naloga3_WPF.ViewModels;

namespace Naloga3_WPF
{
    public partial class MainWindow : Window
    {
        private readonly MainViewModel _vm;

        public MainWindow()
        {
            InitializeComponent();
            _vm = new MainViewModel();
            MainViewModel.MainWindowRef = this;
            DataContext = _vm;
            _vm.NaloziVse();
        }

        private void GridTekmovalci_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _vm.NapolniFormTekmovalec();
        }

        private void GridTekmovanja_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _vm.NapolniFormTekmovanje();
        }

        private void GridUporabniki_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _vm.NapolniFormUporabnik();
        }

        // Getter za geslo (PasswordBox ne podpira bindinga)
        public string GetGeslo() => PbGeslo.Password;
        public void ClearGeslo() => PbGeslo.Password = "";
    }
}
