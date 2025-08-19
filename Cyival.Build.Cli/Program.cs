using Cyival.Build;
using Microsoft.Extensions.Logging;
using Velopack;

VelopackApp.Build().Run();

Console.WriteLine("Hello, World!");

#if DEBUG
BuildApp.LoggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
#endif

using var app = new BuildApp();
app.LoadPlugins();