using System;
using System.ComponentModel;

namespace AuthenticatorPro
{
    public class Account : INotifyPropertyChanged
    {
        public string Name { get; set; }
        public string SecretKey { get; set; }

        private string _Code;
        public string Code
        {
            get
            {
                if (_Code != null)
                    return _Code.ToString().PadLeft(6, '0');
                else
                    return _Code;
            }
        }

        public void RefreshCode()
        {
            _Code = CodeGenerator.ComputeCode(SecretKey);
            NotifyPropertyChanged("Code");
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
