
namespace IzgodnoUserService.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.ConfigureServices();

            var app = builder.Build();
            app.ConfigurePipeline();

            app.Run();
        }
    }
}
