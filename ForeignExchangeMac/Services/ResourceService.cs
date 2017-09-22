namespace ForeignExchangeMac.Services
{
    using Xamarin.Forms;
    public class ResourceService
    {
        // Metodo que optiene los parametros del App.xaml
        public string GetParemeter(string parameterName)
        {
            return Application.Current.Resources[parameterName].ToString();
		}
    }
}
