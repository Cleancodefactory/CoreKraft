# Cookie size reduction
    .AddCookie(options =>
    {
        options.LoginPath = new PathString("/account/signin");
        //Special cookie size reduction
        IDataProtectionProvider dataProtectionProvider = DataProtectionProvider.Create("CoreKraft");
        IDataProtector dataProtector = dataProtectionProvider.CreateProtector("Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationMiddleware");
        options.TicketDataFormat = new OptimizedTicketDataFormat(loggerFactory, dataProtector);
    })

# InMemoryTicketStore
//https://www.red-gate.com/simple-talk/development/dotnet-development/using-auth-cookies-in-asp-net-core/
services.AddTransient<ITicketStore, InMemoryTicketStore>();
services.AddSingleton<IPostConfigureOptions<CookieAuthenticationOptions>, ConfigureCookieAuthenticationOptions>();

# SqliteMemoryTicketStore
services.AddTransient<ITicketStore, SqliteTicketStore>();
services.AddSingleton<IPostConfigureOptions<CookieAuthenticationOptions>, ConfigureCookieAuthenticationOptions>();

# RedisCacheTicketStore
services.AddTransient<ITicketStore, SqliteTicketStore>();
services.AddSingleton<IPostConfigureOptions<CookieAuthenticationOptions>, ConfigureCookieAuthenticationOptions>();
https://stackoverflow.com/questions/53413862/how-to-create-a-persistent-ticket-in-redis