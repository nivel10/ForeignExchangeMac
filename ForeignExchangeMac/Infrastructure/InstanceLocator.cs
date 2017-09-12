namespace ForeignExchangeMac.Infrastructure
{
	using System;
	using ForeignExchangeMac.ViewModels;

    public class InstanceLocator
    {
       public MainViewModel Main
        {
            get;
            set;
        }

        public InstanceLocator()
        {
            Main = new MainViewModel();
        }
    }
}
