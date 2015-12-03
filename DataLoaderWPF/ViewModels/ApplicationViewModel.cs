using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataLoaderWPF.ViewModels;
using System.Windows.Input;
using DataLoaderWPF.Helpers;
using System.Windows;

namespace DataLoaderWPF.ViewModels
{
    public class ApplicationViewModel : ViewModelBase
    {
        private ICommand _finishCommand;
        private ICommand _nextCommand;
        private ICommand _prevCommand;
        private MappingViewModel _mappingVM = null;
        private ConnectionViewModel _connectionVM = null;
        private ViewModelBase _activeModel = null;

        public ApplicationViewModel()
        {
            _connectionVM = new ConnectionViewModel();
            _mappingVM = new MappingViewModel();
            this.ActiveModel = _connectionVM;
        }

        public ICommand FinishCommand
        {
            get
            {
                if (_finishCommand == null)
                {
                    _finishCommand = new RelayCommand(OnFinishCommand, CanFinish);
                }
                return _finishCommand;
            }
        }

        public ICommand NextCommand
        {
            get
            {
                if (_nextCommand == null)
                {
                    _nextCommand = new RelayCommand(OnNextCommand, CanNext);
                }
                return _nextCommand;
            }
        }

        public ICommand PrevCommand
        {
            get
            {
                if (_prevCommand == null)
                {
                    _prevCommand = new RelayCommand(OnPrevCommand, CanPrev);
                }
                return _prevCommand;
            }
        }

        public ViewModelBase ActiveModel
        {
            get
            {
                return _activeModel;
            }
            set
            {
                _activeModel = value;
                NotifyPropertyChanged(() => ActiveModel);
            }
        }

        private bool CanNext(object arg)
        {
            return _connectionVM.GoodConnection && (ActiveModel != _mappingVM);
        }

        private void OnNextCommand(object arg)
        {
            if (ActiveModel == _connectionVM)
            {
                _mappingVM.ConnectionString = _connectionVM.ConnectionString;
                ActiveModel = _mappingVM;
            }
        }

        public override bool IsBusy
        {
            get
            {
                if (base.IsBusy)
                    return true;
                if (this.ActiveModel.IsBusy)
                    return true;

                return false;
            }
            protected set
            {
                base.IsBusy = value;
            }
        }

        private bool CanPrev(object arg)
        {
            return !(ActiveModel is ConnectionViewModel);
        }

        private void OnPrevCommand(object arg)
        {
            if (ActiveModel == _mappingVM)
                ActiveModel = _connectionVM;
        }

        private bool CanFinish(object arg)
        {
            return ActiveModel == _mappingVM && !string.IsNullOrEmpty(_mappingVM.FilesFolder) && System.IO.Directory.Exists(_mappingVM.FilesFolder);
        }

        private void OnFinishCommand(object arg)
        {            
            if (_activeModel == _mappingVM)
            {
                _mappingVM.LoadData();
            }
            NotifyPropertyChanged(() => IsBusy);
        }
    }
}
