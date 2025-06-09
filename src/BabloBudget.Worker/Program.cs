using BabloBudget.Api;

namespace BabloBudget.Worker;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        var configuration = builder.Configuration;

        builder.Services
            .AddLayers(configuration)
            .AddWorker(configuration);

        var app = builder.Build();

        app.UseHttpsRedirection();

        app.UseAuthorization();

        app.Run();
    }
}