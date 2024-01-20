namespace DynDNS.Models.Actions
{
    public class RequestAction<T>
    {
        public string action { get; set; }
        public T param { get; set; }
    }
}
