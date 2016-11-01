namespace ZaloCommunityDev.Shared
{
    public class Filter : NotifyPropertyChanged
    {
        private string _accountName;
        private string _configName;
        private string _excludePeopleNames;
        private string _filterAgeRange;
        private GenderSelection _genderSelection;
        private string _includedPeopleNames;
        private string _locations;
        private int _numberOfAction;
        private string _textGreetingForFemale;
        private string _textGreetingForMale;
        private string _includePhoneNumbers;
        private string _excludePhoneNumbers;
        private string _sentImageForMale;
        private string _sentImageForFemale;

        public string SentImageForMale
        {
            get { return _sentImageForMale; }
            set { _sentImageForMale = value; RaisePropertyChanged(); }
        }

        public string SentImageForFemale
        {
            get { return _sentImageForFemale; }
            set { _sentImageForFemale = value; RaisePropertyChanged(); }
        }

        public string IncludePhoneNumbers
        {
            get { return _includePhoneNumbers; }
            set { _includePhoneNumbers = value; RaisePropertyChanged(); }
        }

        public string ExcludePhoneNumbers
        {
            get { return _excludePhoneNumbers; }
            set { _excludePhoneNumbers = value; RaisePropertyChanged(); }
        }

        public string AccountName
        {
            get { return _accountName; }
            set { _accountName = value; RaisePropertyChanged(); }
        }

        public string ConfigName
        {
            get { return _configName; }
            set { _configName = value; RaisePropertyChanged(); }
        }

        public string ExcludePeopleNames
        {
            get { return _excludePeopleNames; }
            set { _excludePeopleNames = value; RaisePropertyChanged(); }
        }

        public string FilterAgeRange
        {
            get { return _filterAgeRange; }
            set { _filterAgeRange = value; RaisePropertyChanged(); }
        }

        public GenderSelection GenderSelection
        {
            get { return _genderSelection; }
            set { _genderSelection = value; RaisePropertyChanged(); }
        }

        public int Id { get; set; }

        public string IncludedPeopleNames
        {
            get { return _includedPeopleNames; }
            set { _includedPeopleNames = value; RaisePropertyChanged(); }
        }

        public string Locations
        {
            get { return _locations; }
            set { _locations = value; RaisePropertyChanged(); }
        }

        public int NumberOfAction
        {
            get { return _numberOfAction; }
            set { _numberOfAction = value; RaisePropertyChanged(); }
        }

        public string TextGreetingForFemale
        {
            get { return _textGreetingForFemale; }
            set { _textGreetingForFemale = value; RaisePropertyChanged(); }
        }

        public string TextGreetingForMale
        {
            get { return _textGreetingForMale; }
            set { _textGreetingForMale = value; RaisePropertyChanged(); }
        }

        public override string ToString() => $"{ConfigName}";
    }
}