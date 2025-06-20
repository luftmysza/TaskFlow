using TaskFlow.Application.Services;
using TaskFlow.Infrastructure.Config;
using TaskFlow.SOAP.Services;

namespace TaskFlow.SOAP;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddInfrastructure(builder.Configuration, "SOAP");
        
        builder.Services.AddScoped<ICommentsSoapService, CommentsSoapService>();
        
        builder.Services.AddHttpContextAccessor();

        var app = builder.Build();


        app.MapGet("/", () => "Hello World!");

        app.Run();
    }
}
