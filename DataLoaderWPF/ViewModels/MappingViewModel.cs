using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Input;
using DataLoaderWPF.Helpers;
using System.Windows.Forms;
using System.Data;

namespace DataLoaderWPF.ViewModels
{
    public class MappingViewModel : ViewModelBase, IDisposable
    {
        private int _insertProgress = 0;
        private int _tableProgress = 0;
        private int _progress = 0;
        private List<Mapping> _mappings = new List<Mapping>();
        private string _folder = string.Empty;
        private ICommand _selectFolderCommand;
        private string _connectionString = null;

        public MappingViewModel()
        {
            
            Delimiter = null;
            _mappings = new List<Mapping>();
            this.PropertyChanged += MappingViewModel_PropertyChanged;
        }

        public ICommand SelectFolderCommand
        {
            get
            {
                if (_selectFolderCommand == null)
                {
                    _selectFolderCommand = new RelayCommand(OnOpenFolder, CanOpenFolder);
                }
                return _selectFolderCommand;
            }
        }

        public string FilesFolder
        {
            get
            {
                return _folder;
            }
            set
            {
                if (value != _folder)
                {
                    _folder = value;
                    NotifyPropertyChanged(() => FilesFolder);                    
                }
            }
        }

        public string ConnectionString
        {
            get
            {
                return _connectionString;
            }
            set
            {
                if (_connectionString != value)
                {
                    _connectionString = value;
                    NotifyPropertyChanged(() => ConnectionString);
                }
            }
        }

        public int FileProgress
        {
            get
            {
                return _progress;
            }
            set
            {
                if (_progress != value)
                {
                    _progress = value;
                    NotifyPropertyChanged(() => FileProgress);
                }
            }
        }

        public int TableProgress
        {
            get
            {
                return _tableProgress;
            }
            set
            {
                if (_tableProgress != value)
                {
                    _tableProgress = value;
                    NotifyPropertyChanged(() => TableProgress);
                }
            }
        }

        public int InsertProgress
        {
            get
            {
                return _insertProgress;
            }
            set
            {
                if (_insertProgress != value)
                {
                    _insertProgress = value;
                    NotifyPropertyChanged(() => InsertProgress);
                }
            }
        }

        public string Delimiter { get; set; }

        public string ScrubChars { get; set; }

        public List<Mapping> Mappings
        {
            get
            {
                return _mappings;
            }
        }

        public List<DataTable> SourceItems
        {
            get
            {
                return Mappings.Select(a => a.Source).ToList();
            }
        }

        public List<DataTable> DestItems
        {
            get
            {
                return Mappings.Select(a => a.Destination).ToList();
            }
        }

        private void OnFolderSelected()
        {
            if (Directory.Exists(_folder) && !String.IsNullOrEmpty(Delimiter))
            {
            
            }
        }

        private Mapping GetMappingSource(string fileName, string fileContent)
        {
            var sourceOnly = new Mapping();
            var rowTokens = fileContent.Split(new char[] { '\n' });
            var tableName = Path.GetFileNameWithoutExtension(fileName);
            //var columnNames = rowTokens[0].Split(new char[] {  });


            return sourceOnly;
        }

        private string GetFileContent(string file)
        {
            string content = string.Empty;
            using (var fs = File.OpenRead(file))
            {
                using (var streamReader = new StreamReader(fs))
                {
                    content = streamReader.ReadToEnd();
                }
            }
            return content;
        }

        private bool CanOpenFolder(object arg)
        {
            return true;
        }

        public void OnOpenFolder(object arg)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                fbd.RootFolder = Environment.SpecialFolder.MyComputer;
                fbd.Description = "Select the DB files folder";
                if (fbd.ShowDialog() == DialogResult.OK)
                {
                    FilesFolder = fbd.SelectedPath;
                }
            }
        }

        private void MappingViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "FilesFolder":
                    if (!string.IsNullOrEmpty(FilesFolder) && Directory.Exists(FilesFolder))
                    {
                        OnFolderSelected();
                    }
                    break;
            }
        }

        public void Dispose()
        {
            foreach (var mapping in Mappings)
            {
                if (mapping.Source != null)
                    mapping.Source.Dispose();

                if (mapping.Destination != null)
                    mapping.Destination.Dispose();
            }
        }

        internal void LoadData()
        {
            ExecuteAsync(() =>
            {
                var files = System.IO.Directory.GetFiles(FilesFolder);
                var scrubChars = ScrubChars.ToCharArray();
                var delimtChar = '$';
                if (!string.IsNullOrEmpty(Delimiter))
                    delimtChar = Delimiter[0];
                var dl = new DataLoader(ConnectionString, files, delimtChar, scrubChars);
                dl.FileProgressHappened += dl_FileProgressHappened;
                dl.TableProgressHappened += dl_TableProgressHappened;
                dl.InsertProgressHappened += dl_InsertProgressHappened;     
                dl.LoadData();
                dl.InsertProgressHappened -= dl_InsertProgressHappened;     
                dl.FileProgressHappened -= dl_FileProgressHappened;
                dl.TableProgressHappened -= dl_TableProgressHappened;
            }, () =>
            {
                MessageBox.Show("Complete");
            });
        }

        void dl_InsertProgressHappened(int obj)
        {
            this.InsertProgress = obj;
        }

        void dl_TableProgressHappened(int obj)
        {
            this.TableProgress = obj;
        }

        void dl_FileProgressHappened(int obj)
        {
            this.FileProgress = obj;
        }

        void dl_ProgressHappened(int obj)
        {
            this.FileProgress = obj;
        }
    }
}
