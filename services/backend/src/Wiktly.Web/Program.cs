using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Wiktly.Web.AppInitialization;

var builder = AppInitialization.Initialize(args);
var configuration = builder.Configuration;

// Add services to the container.
builder.Services.AddRazorPages(o =>
{
});
builder.Services.AddControllers();

using var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseApiVersioning(configuration);
app.UseCors();

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();
app.MapControllers();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();
