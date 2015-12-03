using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using DataLoaderWPF.Helpers;
using DataLoaderWPF.ViewModels;
using DataLoaderWPF.VIews;

namespace DataLoaderWPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            ViewMapper.Map<ConnectionViewModel, ConnectionView>(Application.Current.Resources);
            ViewMapper.Map<MappingViewModel, MappingView>(Application.Current.Resources);
        }
    }
}
