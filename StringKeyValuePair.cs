using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimetableFH
{
    public class StringKeyValuePair : INotifyPropertyChanged
    {
        private string key, value;

        public string Key
        {
            get => key;
            set
            {
                if (value == key) return;

                key = value;
                OnPropertyChanged(nameof(Key));
            }
        }

        public string Value
        {
            get => value;
            set
            {
                if (value == this.value) return;

                this.value = value;
                OnPropertyChanged(nameof(Value));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
