using System.Runtime.InteropServices;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddControllersWithViews();
builder.Services.AddDirectoryBrowser();

var app = builder.Build();

var envKv = app.Configuration.AsEnumerable().ToList();

Console.WriteLine("环境变量 开始");
foreach (var keyValuePair in envKv)
{
    Console.WriteLine(keyValuePair.Key + "=" + keyValuePair.Value);
}

Console.WriteLine("环境变量 结束");

bool isOpenDirectoryBrowser = false;
string rootPath = "./Bundles";
foreach (var keyValuePair in envKv)
{
    if (keyValuePair.Key.Equals("IsOpenDirectoryBrowser"))
    {
        isOpenDirectoryBrowser = bool.Parse(keyValuePair.Value);
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseDefaultFiles();


var requestPath = "/Bundles";
if (!Directory.Exists(requestPath))
{
    if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
    {
        Directory.CreateDirectory(rootPath, UnixFileMode.UserWrite | UnixFileMode.UserRead);
    }
    else
    {
        Directory.CreateDirectory(rootPath);
    }
}

var provider = new FileExtensionContentTypeProvider
{
    Mappings =
    {
        ["*.*"] = "application/octet-stream"
    }
};

app.UseStaticFiles(new StaticFileOptions()
{
    ContentTypeProvider = provider,
    ServeUnknownFileTypes = true,
    DefaultContentType = "application/octet-stream"
});

if (isOpenDirectoryBrowser)
{
    PhysicalFileProvider fileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), rootPath));
    app.UseDirectoryBrowser(new DirectoryBrowserOptions
    {
        FileProvider = fileProvider,
        RequestPath = requestPath,
    });
}

app.MapDefaultControllerRoute();
app.MapRazorPages();

app.Run();