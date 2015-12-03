using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Input;
using DataLoaderWPF.Helpers;

namespace DataLoaderWPF.ViewModels
{
    public class ConnectionViewModel : ViewModelBase
    {
        private ICommand _testConnection;
        private string _server;
        private string _database;
        private bool _windowsAuthentication = true;
        private string _username;
        private string _password;
        private string _connectionString;
        private bool _connectionSuccess = false;
        private string _connectionFailMsg = string.Empty;

        public ICommand TestConnectionCommand
        {
            get
            {
                if (_testConnection == null)
                {
                    _testConnection = new RelayCommand(TestConnection, CanTestConnection);
                }
                return _testConnection;
            }
        }

        public string ErrorMsg
        {
            get
            {
                return _connectionFailMsg;
            }
        }

        public string Server
        {
            get
            {
                return _server;
            }
            set
            {
                _server = value;
                NotifyPropertyChanged(() => Server);
            }
        }

        public string Database
        {
            get
            {
                return _database;
            }
            set
            {
                _database = value;
                NotifyPropertyChanged(() => Database);
            }
        }

        public bool WindowsAuthentication
        {
            get
            {
                return _windowsAuthentication;
            }
            set
            {
                _windowsAuthentication = value;
                NotifyPropertyChanged(() => WindowsAuthentication);
                switch (value)
                {
                    case true:
                        Username = string.Empty;
                        Password = string.Empty;
                        break;
                }
            }
        }

        public string Username
        {
            get
            {
                return _username;
            }
            set
            {
                _username = value;
                NotifyPropertyChanged(() => Username);
            }
        }

        public string Password
        {
            get
            {
                return _password;
            }
            set
            {
                _password = value;
                NotifyPropertyChanged(() => Password);
            }
        }

        public string ConnectionString
        {
            get
            {
                return _connectionString;
            }
        }

        public bool GoodConnection
        {
            get
            {
                return _connectionSuccess && !IsBusy;
            }
        }

        public bool CanTestConnection(object arg)
        {
            if (IsBusy)
                return false;

            if (WindowsAuthentication)
            {
                return !string.IsNullOrEmpty(Server) && !string.IsNullOrEmpty(Database);
            }

            return !string.IsNullOrEmpty(Server) &&
                   !string.IsNullOrEmpty(Database) && 
                   !string.IsNullOrEmpty(Username) && 
                   !string.IsNullOrEmpty(Password);
        }

        public void TestConnection(object arg)
        {
            ExecuteAsync(() =>
            {
                _connectionSuccess = false;
                var standardConnectionString = string.Format("Server={0};Database={1};User Id={2};Password={3};", Server, Database, Username, Password);
                var waConnectionstring = string.Format("Server={0};Database={1};Trusted_Connection=True;", Server, Database);

                SqlConnection conn = new SqlConnection();
                switch (WindowsAuthentication)
                {
                    case false:
                        _connectionString = standardConnectionString;
                        break;
                    default:
                        _connectionString = waConnectionstring;
                        break;
                }
                try
                {
                    conn.ConnectionString = _connectionString;
                    conn.Open();
                }
                catch (Exception ex)
                {
                    _connectionSuccess = false;
                    _connectionFailMsg = ex.Message;
                    return;
                }
                finally
                {
                    conn.Dispose();
                }
                _connectionSuccess = true;
            }, () =>
            {
                NotifyPropertyChanged("ErrorMsg");
                NotifyPropertyChanged("IsBusy");
                NotifyPropertyChanged("GoodConnection");
                NotifyPropertyChanged("Server");
            });
        }
    }
}
