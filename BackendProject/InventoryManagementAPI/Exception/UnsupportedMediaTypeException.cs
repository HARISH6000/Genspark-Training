namespace InventoryManagementAPI.Exceptions
{
    public class UnsupportedMediaTypeException : Exception
    {
        public UnsupportedMediaTypeException() { }

        public UnsupportedMediaTypeException(string message) : base(message) { }

        public UnsupportedMediaTypeException(string message, Exception innerException) : base(message, innerException) { }
    }
}