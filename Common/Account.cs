using System;
using System.Windows.Media;
using System.ComponentModel;
using System.Xml.Serialization;

namespace Authenticator
{
    public class Account : INotifyPropertyChanged
    {
        public string AccountName { get; set; }
        public string SecretKey { get; set; }

        private string _Code;
        public string Code
        {
            get
            {
                return _Code;
            }

            set
            {
                _Code = value;
                NotifyPropertyChanged("Code");
            }
        }

        private string _Message;

        [XmlIgnore]
        public string Message
        {
            get
            {
                return _Message;
            }
            set
            {
                _Message = value;
                NotifyPropertyChanged("Message");
            }
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
