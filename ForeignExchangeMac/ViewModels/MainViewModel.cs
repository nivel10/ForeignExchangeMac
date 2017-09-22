namespace ForeignExchangeMac.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Threading.Tasks;
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
        private List<Rate> _ratesList;

        #region Services

        private ApiService apiService;
        private DataService dataService;
        private DialogService dialogService;
        private ResourceService resourceService;

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
            // Servicios instanciados   
            apiService = new ApiService();
            dataService = new DataService();
            dialogService = new DialogService();
            resourceService = new ResourceService();

            //  Invoca el metodo carga de tasas
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
                await dialogService.ShowMessage(
                    Lenguages.Error, Lenguages.AmountValidation);
                return;
            }

            decimal amount = 0;
            if(!decimal.TryParse(Amount, out amount))
			{
                await dialogService.ShowMessage(
                    Lenguages.Error, Lenguages.AmountNumericValidation);
                return;
            }

            if(SourceRate == null)
			{
				await dialogService.ShowMessage(
                    Lenguages.Error, Lenguages.SourceRateValidation);
				return;
			}

			if (TargetRate == null)
			{
				await dialogService.ShowMessage(
                    Lenguages.Error, Lenguages.TargetRateValidation);
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
                //  Establece los titulos e inicia el ActivityIndicator
				IsRunning = true;
                Result = Lenguages.TitleLoadRate;

                //  Verifica si hay conexion a internet 
                var connection = await apiService.CheckConnection();
                if(!connection.IsSuccess)
                {
                    //  Invoca el metodo de carga local (Tasas)
                    LoadLocalData();

                    IsRunning = false;
                    IsEnabled = false;
                    Result = connection.Messages;
                    return;
                }
                else
                {
                    // Invoca el metodo que hace la carga de tasas WepApi
                    await LoadDataFromAPI();
                }

                //  Valida el resultado de los metodos anteriores
                if(_ratesList.Count ==0)
                {
                    IsEnabled = false;
                    IsRunning = false;
                    Result = Lenguages.TitleStatusLoadError;
                    Status = Lenguages.TitleStatusNoLoad;
                    return;
                }

				//  Carga la ObservableCollection
				Rates = new ObservableCollection<Rate>(_ratesList);

                IsRunning = false;
                IsEnabled = true;
                Result = Lenguages.TitleReadyConvert;
			}
            catch (Exception ex)
            {
                IsRunning = false;
                Result = ex.Message.Trim();
            }
        }

        /// <summary>
        /// Metodo que hace la carga de las tasas locales (SQLite)
        /// </summary>
        private void LoadLocalData()
        {
            _ratesList = dataService.Get<Rate>(false);
            Status = Lenguages.TitleStatusLoadLocal;
        }

        /// <summary>
        /// Metodo que hqce la carga de datos de WepAPI
        /// </summary>
        /// <returns>The data from API.</returns>
        private async Task LoadDataFromAPI()
        {
			// Optiene las tasas del WebService
			var respose = await apiService.GetList<Rate>(
				resourceService.GetParemeter("UrlAPI"), "api/Rates");
			if (!respose.IsSuccess)
			{
                //  Invoca el metodo de carga local de tasas
                LoadLocalData();
				return;
			}

			// Crea un objeto de tipo List<>
			_ratesList = (List<Rate>)respose.Result;
			//  Elimina todas las tasas locales
			dataService.DeleteAll<Rate>();
			//  Salva todas las tasas (Captura un obejto de tipo List)
			dataService.Save(_ratesList);
            Status = Lenguages.TitleSettingsInternet;
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
