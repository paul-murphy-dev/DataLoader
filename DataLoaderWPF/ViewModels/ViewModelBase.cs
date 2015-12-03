using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataLoaderWPF.ViewModels
{
    public class ViewModelBase : INotifyPropertyChanged
    {
        private Dictionary<string, string> _propLookup = null;
        public event PropertyChangedEventHandler PropertyChanged;
        private bool _isBusy = false;
        private Action _thingCompleted;

        public ViewModelBase()
        {
            _propLookup = new Dictionary<string, string>(this.GetType().GetProperties().Count());
        }

        public virtual bool IsBusy
        {
            get
            {
                return _isBusy;
            }
            protected set
            {
                if (value != _isBusy)
                {
                    _isBusy = value;
                    NotifyPropertyChanged(() => IsBusy);
                }
            }
        }

        protected void ExecuteAsync(Action ThingToDo, Action OnThingCompleted)
        {
            IsBusy = true;
            _thingCompleted = OnThingCompleted;
            ThingToDo.BeginInvoke(this.OnAsyncCompleted, null);
        }

        protected void OnAsyncCompleted(IAsyncResult result)
        {
            IsBusy = false;
            if (_thingCompleted != null)
                _thingCompleted();    
        }

        public void NotifyPropertyChanged(Func<object> v)
        {
            string name = null;
            string raw = v.Method.Name;
            if (_propLookup.ContainsKey(raw))
            {
                name = _propLookup[raw];
            }
            else
            {
                try
                {
                    int start = raw.IndexOf("_") + 1;
                    int length = raw.LastIndexOf(">") - start;
                    name = v.Method.Name.Substring(start, length);
                }
                catch
                {
                    return;//do nothing
                }
            }
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        public void NotifyPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }
    }
}
