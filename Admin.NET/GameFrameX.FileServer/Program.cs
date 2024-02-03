using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddControllersWithViews();

builder.Services.AddDirectoryBrowser();

var app = builder.Build();


if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}


app.UseDefaultFiles();
 
var envKv=  app.Configuration.AsEnumerable().ToList();
string RootPath = string.Empty;
foreach (var keyValuePair in envKv)
{
    if (keyValuePair.Key.Equals("RootPath"))
    {
        RootPath = keyValuePair.Value;
    }
}

var fileProvider = new PhysicalFileProvider(Path.Combine(builder.Environment.WebRootPath, "Bundles"));
if (!string.IsNullOrWhiteSpace(RootPath))
{
    fileProvider = new PhysicalFileProvider(RootPath);
}

var requestPath = "/Bundles";

app.UseStaticFiles(new StaticFileOptions()
{
    //设置不限制content-type
    ServeUnknownFileTypes = true,
});
app.UseDirectoryBrowser(new DirectoryBrowserOptions
{
    FileProvider = fileProvider,
    RequestPath = requestPath
});

app.MapDefaultControllerRoute();
app.MapRazorPages();

app.Run();