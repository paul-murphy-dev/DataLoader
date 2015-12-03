using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using DataLoaderWPF.ViewModels;
using System.ComponentModel;

namespace DataLoaderWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public ViewModelBase ViewModel { get; set; }

        public MainWindow()
        {
            InitializeComponent();

            this.DataContext = this;
            this.ViewModel = new ApplicationViewModel();
        }        
    }
}
