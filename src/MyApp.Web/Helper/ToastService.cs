public class ToastService
{
    // Gunakan Func<Task> agar async-safe
    public event Func<ToastInstance, Task>? OnShow;

    public Task ShowError(string message, int duration = 5000)
    {
        return OnShow?.Invoke(new ToastInstance
        {
            Title = "Notification Error !",
            Message = message,
            Type = "error",
            Duration = duration
        }) ?? Task.CompletedTask;
    }

    public Task ShowInfo(string message, int duration = 5000)
    {
        return OnShow?.Invoke(new ToastInstance
        {
            Title = "Notification Information !",
            Message = message,
            Type = "info",
            Duration = duration
        }) ?? Task.CompletedTask;
    }

    public Task ShowWarning(string message, int duration = 5000)
    {
        return OnShow?.Invoke(new ToastInstance
        {
            Title = "Notification Warning !",
            Message = message,
            Type = "warning",
            Duration = duration
        }) ?? Task.CompletedTask;
    }

    public Task ShowSuccess(string message, int duration = 5000)
    {
        return OnShow?.Invoke(new ToastInstance
        {
            Title = "Notification Success !",
            Message = message,
            Type = "success",
            Duration = duration
        }) ?? Task.CompletedTask;
    }
}


public class ToastInstance
{
    public Guid Id { get; set; } = Guid.NewGuid();  // tambahkan ini
    public string Title { get; set; } = "";
    public string Message { get; set; } = "";
    public string Type { get; set; } = "info";
    public int Duration { get; set; } = 5000;
    public bool IsFadingOut { get; set; } = false;   // untuk animasi fade
}

