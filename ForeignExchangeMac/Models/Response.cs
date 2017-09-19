namespace ForeignExchangeMac.Models
{
    public class Response
    {
        public bool IsSuccess
        {
            get;
            set;
        }

        public string Messages
        {
            get;
            set;
       }

        public object Result
        {
            get;
            set;
        }
    }
}
