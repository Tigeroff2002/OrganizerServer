using ToDoCalendarServer;

await Host.CreateDefaultBuilder(args)
    .ConfigureWebHostDefaults(webBuilder =>
        webBuilder.UseStartup<Startup>())
    .Build()
    .RunAsync()
    .ConfigureAwait(false);