using AspNetCore.Proxy;
using FluentValidation.AspNetCore;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Pondrop.Service.ShoppingList.Api.Configurations.Extensions;
using Pondrop.Service.ShoppingList.Api.Middleware;
using Pondrop.Service.ShoppingList.Api.Services;
using Pondrop.Service.ShoppingList.Api.Services.Interfaces;
using Pondrop.Service.ShoppingList.Application.Models;
using Pondrop.Service.ShoppingList.Domain.Models;
using Pondrop.Service.Infrastructure.CosmosDb;
using Pondrop.Service.Infrastructure.Dapr;
using Pondrop.Service.Infrastructure.ServiceBus;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Pondrop.Service.Interfaces;
using Pondrop.Service.Models;
using Pondrop.Service.Interfaces.Services;
using Pondrop.Service.Product.Application.Models;
using Pondrop.Service.Events;
using Pondrop.Service.ShoppingList.Domain.Events.ShoppingList;

JsonConvert.DefaultSettings = () => new JsonSerializerSettings()
{
    ContractResolver = new DefaultContractResolver()
    {
        NamingStrategy = new CamelCaseNamingStrategy()
    },
    DateTimeZoneHandling = DateTimeZoneHandling.Utc
};

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;
var configuration = builder.Configuration;

builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile("appsettings.local.json", optional: true, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName.ToLowerInvariant()}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

services.Configure<JsonOptions>(options =>
{
    options.JsonSerializerOptions.PropertyNameCaseInsensitive = false;
    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
});

services.AddProxies();

// Add services to the container.
services.AddVersionedApiExplorer(options => options.GroupNameFormat = "'v'VVV");
services.AddApiVersioning(options =>
{
    options.ReportApiVersions = true;
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
});

services.AddLogging(config =>
{
    config.AddDebug();
    config.AddConsole();
});
services
    .AddControllers()
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
services.AddEndpointsApiExplorer();

services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme. \r\n\r\n Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\nExample: \"Bearer 1safsfsdfdfd\"",
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement {
        {
            new OpenApiSecurityScheme {
                Reference = new OpenApiReference {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});
services.AddAutoMapper(
    typeof(Result<>),
    typeof(EventEntity),
    typeof(EventRepository),
    typeof(CreateShoppingList));
services.AddMediatR(
    typeof(Result<>));
services.AddFluentValidation(config =>
    {
        config.RegisterValidatorsFromAssemblyContaining(typeof(Result<>));
    });


services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(o =>
{
    var Key = Encoding.UTF8.GetBytes(configuration["JWT:Key"]);
    o.SaveToken = true;
    o.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = configuration["JWT:Issuer"],
        ValidAudience = configuration["JWT:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Key)
    };
});

services.Configure<CosmosConfiguration>(configuration.GetSection(CosmosConfiguration.Key));
services.Configure<ServiceBusConfiguration>(configuration.GetSection(ServiceBusConfiguration.Key));
services.Configure<SharedListShopperUpdateConfiguration>(configuration.GetSection(DaprEventTopicConfiguration.Key).GetSection(SharedListShopperUpdateConfiguration.Key));
services.Configure<ShoppingListUpdateConfiguration>(configuration.GetSection(DaprEventTopicConfiguration.Key).GetSection(ShoppingListUpdateConfiguration.Key));
services.Configure<ListItemUpdateConfiguration>(configuration.GetSection(DaprEventTopicConfiguration.Key).GetSection(ListItemUpdateConfiguration.Key));

services.AddHostedService<ServiceBusHostedService>();
services.AddSingleton<IServiceBusListenerService, ServiceBusListenerService>();

services.AddHostedService<RebuildMaterializeViewHostedService>();
services.AddSingleton<IRebuildCheckpointQueueService, RebuildCheckpointQueueService>();

services.AddSingleton<IAddressService, AddressService>();
var serviceCollection = services.AddSingleton<IUserService, UserService>();
services.AddSingleton<IEventRepository, EventRepository>();
services.AddSingleton<ICheckpointRepository<SharedListShopperEntity>, CheckpointRepository<SharedListShopperEntity>>();
services.AddSingleton<ICheckpointRepository<ListItemEntity>, CheckpointRepository<ListItemEntity>>();
services.AddSingleton<ICheckpointRepository<ShoppingListEntity>, CheckpointRepository<ShoppingListEntity>>();
services.AddSingleton<IDaprService, DaprService>();
services.AddSingleton<IServiceBusService, ServiceBusService>();
services.AddSingleton<ITokenProvider, JWTTokenProvider>();

DefaultEventTypePayloadResolver.Init(typeof(CreateShoppingList).Assembly);

var app = builder.Build();
var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();

// Configure the HTTP request pipeline.

app.UseMiddleware<ExceptionMiddleware>();
app.UseSwaggerDocumentation(provider);

app.UseAuthentication();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
