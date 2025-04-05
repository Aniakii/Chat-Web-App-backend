using Amazon.S3;
using FormulaOne.ChatService.DataService;
using FormulaOne.ChatService.Hubs;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddSignalR();
builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDefaultAWSOptions(builder.Configuration.GetAWSOptions());
builder.Services.AddAWSService<IAmazonS3>();

builder.Services.AddScoped<IFileService, S3FileService>();

var publicIp = Environment.GetEnvironmentVariable("PUBLIC_IP") ?? "localhost";

builder.Services.AddCors(opt =>
{
    opt.AddPolicy("AllowSpecificOrigin", builder =>
    {
        builder.WithOrigins("http://192.168.1.103:8080", "http://localhost:8080",
            "http://localhost:3000", "http://156.17.237.167:80",
            "http://amajkafront.us-east-1.elasticbeanstalk.com/80",
            "amajkafront.us-east-1.elasticbeanstalk.com:80", "http://127.0.0.1:3000", $"http://{publicIp}:80")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

builder.Services.AddSingleton<SharedDb>();

builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.ListenAnyIP(80);
});

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseSwagger();
app.UseSwaggerUI();


//app.UseHttpsRedirection();



app.UseRouting();

app.UseCors("AllowSpecificOrigin");

app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
    endpoints.MapHub<ChatHub>("/Chat");
});


app.Run();
