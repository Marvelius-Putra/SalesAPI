using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SalesAPI.Interfaces;
using SalesAPI.Model;
using SalesAPI.Repositories;
using System.IO;
using System.Reflection;


var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureAppConfiguration(config =>
    {
        config.SetBasePath(Environment.CurrentDirectory)
        .AddJsonFile("appsettings.json", true)
        .AddJsonFile("localsettings.json", true);
    })
    .ConfigureServices(services =>
    {
        services.AddOptions<AppSettings>()
         .Configure<IConfiguration>((settings, configuration) => configuration.GetSection("AppSettings").Bind(settings));
        // Registrasi dependency injection
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<ISupplierRepository, SupplierRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<ISalesRepository, SalesRepository>();

    })
    .Build();

host.Run();
