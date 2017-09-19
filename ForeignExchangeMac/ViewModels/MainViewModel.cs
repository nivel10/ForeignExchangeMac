namespace ForeignExchangeMac.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Windows.Input;
    using ForeignExchangeMac.Helpers;
    using ForeignExchangeMac.Services;
    using GalaSoft.MvvmLight.Command;
    using Models;
    using Newtonsoft.Json;
    using Xamarin.Forms;

    public class MainViewModel : INotifyPropertyChanged
    {
        #region Attributes

        private bool _isRunning;
        private string _result;
        private bool _isEnabled;
        private ObservableCollection<Rate> _rates;
        private Rate _sourceRate;
        private Rate _targetRate;
        private string _status;

        #region Services

        private ApiService apiService;

        #endregion

        #endregion

        #region Commands

        public ICommand ConvertCommand
        {
            get
            {
                return new RelayCommand(Convert);
            }
        }

        public ICommand SwitchCommand
        {
            get
            {
                return new RelayCommand(Switch);
            }
        }

		#endregion

		#region Events

		public event PropertyChangedEventHandler PropertyChanged;

		#endregion

		#region Properties

		public string Amount
        {
            get;
            set;
        }

        public ObservableCollection<Rate> Rates
        {
            get
			{
                return _rates;
            }
			set
			{
                if(value != _rates)
				{
                    _rates = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Rates)));
                }
			}
		}

        public Rate SourceRate
        {
            get
            {
                return _sourceRate;
            }
            set
            {
                if(value != _sourceRate)
                {
                    _sourceRate = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SourceRate)));
                }
            }
        }

        public Rate TargetRate
        {
            get
            {
                return _targetRate;
            }
            set
            {
                if(value !=_targetRate)
                 {
                    _targetRate = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TargetRate)));
                }
            }
        }

        public bool IsRunning
        {
            get
            {
                return _isRunning;
            }
			set
			{
                if(value != _isRunning)
				{
                    _isRunning = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsRunning)));
                }
			}
		}

        public bool IsEnabled
        {
            get
			{
                return _isEnabled;
            }

			set
			{
                if(value != _isEnabled)
				{
                    _isEnabled = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsEnabled)));
                }
			}
		}

        public string Result
        {
            get
			{
                return _result;
            }
			set
			{
                if(value != _result)
				{
                    _result = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Result)));
                }
			}
		}

        public string Status
        {
            get
            {
                return _status;
            }
            set
            {
                if(value != _status)
                {
                    _status = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Status))); 
                }
            }
        }

        #endregion

        #region Constructor

        public MainViewModel()
        {
            apiService = new ApiService();
            LoadRates();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Metodo que hace la conversion de las tasas
        /// </summary>
        private async void Convert()
        {
            if(string.IsNullOrEmpty(Amount))
			{
                await Application.Current.MainPage.DisplayAlert(
                    Lenguages.Error, Lenguages.AmountValidation, Lenguages.Accept);
                return;
            }

            decimal amount = 0;
            if(!decimal.TryParse(Amount, out amount))
			{
                await Application.Current.MainPage.DisplayAlert(
                    Lenguages.Error, Lenguages.AmountNumericValidation, Lenguages.Accept);
                return;
            }

            if(SourceRate == null)
			{
				await Application.Current.MainPage.DisplayAlert(
                    Lenguages.Error, Lenguages.SourceRateValidation, Lenguages.Accept);
				return;
			}

			if (TargetRate == null)
			{
				await Application.Current.MainPage.DisplayAlert(
                    Lenguages.Error, Lenguages.TargetRateValidation, Lenguages.Accept);
				return;
			}

            var amountConvert = (amount / (decimal)SourceRate.TaxRate) 
                * (decimal)TargetRate.TaxRate;

            Result = string.Format("{0} {1:N2} = {2} {3:N2}", 
                                   SourceRate.Code.Trim(),
                                   amount,
                                   TargetRate.Code,
                                   amountConvert);
		}

        /// <summary>
        /// Metodo que hace la carga de las tasas
        /// </summary>
		private async void LoadRates()
		{
            try
            {
				IsRunning = true;
                //  Result = "Load rates, please wait...!!!";
                Result = Lenguages.TitleLoadRate;

                var respose = await apiService.GetList<Rate>("http://apiexchangerates.azurewebsites.net", "api/Rates");
                if(!respose.IsSuccess)
                {
					IsRunning = false;
                    IsEnabled = false;
                    await Application.Current.MainPage.DisplayAlert(
                        Lenguages.Error, respose.Messages, Lenguages.Accept);
                    return;
                }

                Rates = new ObservableCollection<Rate>((List<Rate>)respose.Result);
                IsRunning = false;
                IsEnabled = true;
                //  Result = "Ready to convert...!!!";
                Result = Lenguages.TitleReadyConvert;
			}
            catch (Exception ex)
            {
                IsRunning = false;
                Result = ex.Message.Trim();
            }
        }

        /// <summary>
        /// Metodo que hace el cambio de tasas
        /// </summary>
        private void Switch()
        {
            var aux = SourceRate;
            SourceRate = TargetRate;
            TargetRate = aux;

            Convert();
        }

		#endregion
    }
}
