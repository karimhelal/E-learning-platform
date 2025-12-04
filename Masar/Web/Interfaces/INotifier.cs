namespace Web.Interfaces
{
    public interface INotifier
    {
        Task SendToAdminsAsync(string title, string message, string url);
    }
}
