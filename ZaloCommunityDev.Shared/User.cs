namespace ZaloCommunityDev.Shared
{
    public class User : NotifyPropertyChanged
    {
        private string _username;
        private string _password;
        private bool _isActive = true;
        private string _region = "Vietnam";
        private int _order;

        public string Username
        {
            get { return _username; }
            set
            {
                _username = value;
                RaisePropertyChanged();
            }
        }

        public string Password
        {
            get { return _password; }
            set
            {
                _password = value;
                RaisePropertyChanged();
            }
        }

        public string Region
        {
            get { return _region; }
            set
            {
                _region = value;
                RaisePropertyChanged();
            }
        }

        public bool IsActive
        {
            get { return _isActive; }
            set
            {
                _isActive = value;
                RaisePropertyChanged();
            }
        }

        public int Order
        {
            get
            {
                return _order;
            }

            set
            {
                _order = value;
                RaisePropertyChanged();
            }
        }
    }
}