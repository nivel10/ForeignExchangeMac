namespace ForeignExchangeMac.ViewModels
{
    using System;
    using System.Collections.ObjectModel;
    using System.Windows.Input;
    using GalaSoft.MvvmLight.Command;
    using Models;

    public class MainViewModel
    {
        #region Commands

        public ICommand ConvertCommand
        {
            get
            {
                return new RelayCommand(Convert);
            }
        }

        #endregion

        #region Properties

        public string Amount
        {
            get;
            set;
        }

        public ObservableCollection<Rate> Rates
        {
            get;
            set;
        }

        public Rate SourceRate
        {
            get;
            set;
        }

        public Rate TargetRate
        {
            get;
            set;
        }

        public bool IsRunning
        {
            get;
            set;
        }

        public bool IsEnabled
        {
            get;
            set;
        }

        public string Result
        {
            get;
            set;
        }

        #endregion

        #region Constructor

        public MainViewModel()
        {
        }

        #endregion

        #region Methods

        private void Convert()
        {
            throw new NotImplementedException();
        }

		#endregion
    }
}
