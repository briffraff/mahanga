namespace mahanga.Core.Interface
{
    public interface IAMService
    {
        void CheckAndResetWindowSize();
        string CheckExeLocation();
        string GetExeLocation(string location);
    }
}