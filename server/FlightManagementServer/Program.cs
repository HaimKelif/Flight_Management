using System.Data.SqlClient;



IConfiguration Configuration = new ConfigurationBuilder()
                            .AddJsonFile("appsettings.json")
                            .Build(); 


var builder = WebApplication.CreateBuilder(args);
builder.Services.AddCors(options => options.AddPolicy("ApiCorsPolicy", builder =>
{
    builder.WithOrigins(Configuration["cors:allowedHosts"]).AllowAnyMethod().AllowAnyHeader().AllowCredentials();
}));


builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<SqlConnection>(provider =>
{
    var configuration = provider.GetRequiredService<IConfiguration>();
    return new SqlConnection(configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.AddScoped<GeneralSqlRepository>();

builder.Services.AddHostedService<FlightUpdateService>();


builder.Services.AddSignalR();
var app = builder.Build();
app.UseCors("ApiCorsPolicy");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}




app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapHub<FlightHub>("/flightHub");


app.Run();
