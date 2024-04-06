using ToDoCalendarServer;

await Host.CreateDefaultBuilder(args)
    .ConfigureWebHostDefaults(webBuilder =>
        webBuilder
        .UseIISIntegration()
        .UseStartup<Startup>()
        .CaptureStartupErrors(true)
        .UseSetting("detailedErrors", "true"))
    .Build()
    .RunAsync()
    .ConfigureAwait(false);