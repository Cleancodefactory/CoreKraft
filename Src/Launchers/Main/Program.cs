using Ccf.Ck.Launchers.Main;
using Ccf.Ck.Launchers.Main.ActionFilters;
using Ccf.Ck.Launchers.Main.Routing;
using Ccf.Ck.Launchers.Main.Utils;
using Ccf.Ck.Libs.Logging;
using Ccf.Ck.Libs.SendEmailExtended;
using Ccf.Ck.Models.Settings;
using Ccf.Ck.SysPlugins.Interfaces;
using Ccf.Ck.Web.Middleware;
using Ccf.Ck.Web.Middleware.Aws;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;

string[] _Args = args;
bool _RestartRequest = false;
CancellationTokenSource _CancellationToken = new CancellationTokenSource();

do
{
    _RestartRequest = false;
    _CancellationToken = new CancellationTokenSource();

    var contentRoot = _Args.Length > 0
        ? Path.GetFullPath(_Args[0])
        : Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
    Console.WriteLine("Content root: " + contentRoot);

    WebApplicationBuilder builder = WebApplication.CreateBuilder(new WebApplicationOptions
    {
        Args = _Args,
        ContentRootPath = contentRoot
    });

    builder.Logging.ClearProviders();

    // Load configuration
    builder.Configuration
        .SetBasePath(builder.Environment.ContentRootPath)
        .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: false, reloadOnChange: true)
        .AddEnvironmentVariables();

    AwsConfiguration awsConfig = new AwsConfiguration();
    builder.Configuration.GetSection("AwsConfiguration").Bind(awsConfig);
    if (!string.IsNullOrEmpty(awsConfig?.Name) && !string.IsNullOrEmpty(awsConfig.Region))
    {
        var awsBuilder = new ConfigurationBuilder();
        awsBuilder.Add(new AmazonSecretsManagerConfigurationSource(awsConfig.Region, awsConfig.Name));
        var awsConfiguration = awsBuilder.Build();

        // Merge secrets into the main configuration
        builder.Configuration.AddConfiguration(awsConfiguration);
    }

    IServiceCollection services = builder.Services;
    ConfigurationManager config = builder.Configuration;

    IServiceProvider serviceProvider = services.UseBindKraft(config);
    KraftGlobalConfigurationSettings kraftSettings = serviceProvider.GetService<KraftGlobalConfigurationSettings>();

    EmailSettings emailSettings = new EmailSettings();
    config.GetSection("EmailSettings").Bind(emailSettings);
    services.AddSingleton(emailSettings);

    if (kraftSettings.GeneralSettings.RazorAreaAssembly.IsConfigured && kraftSettings.GeneralSettings.RazorAreaAssembly.IsEnabled)
    {
        ConfigureCookiePolicy(services);
        services.AddControllersWithViews(options =>
        {
            options.Filters.Add(typeof(CultureActionFilter));
        }).ConfigureApplicationPartManager(apm =>
        {
            ConfigureApplicationParts(apm, kraftSettings);
        }).AddTagHelpersAsServices();
        services.AddSingleton<DynamicHostRouteTransformer>();
    }
    else if (kraftSettings.GeneralSettings.BlazorAreaAssembly.IsConfigured && kraftSettings.GeneralSettings.BlazorAreaAssembly.IsEnabled)
    {
        var startupBlazor = new StartupBlazorDynamicAssembly(kraftSettings);
        startupBlazor.LoadBlazorAssembly();
        ConfigureCookiePolicy(services);
        startupBlazor.ConfigureServices(builder);
        services.AddSingleton(startupBlazor); // Save for later use in app
    }
    else
    {
        services.AddControllersWithViews(options =>
        {
            options.Filters.Add(typeof(CultureActionFilter));
        });
    }

    //services.AddAntiforgery(options =>
    //{
    //    options.Cookie.Name = "XSRF-TOKEN";
    //    options.HeaderName = "X-XSRF-TOKEN";
    //    options.FormFieldName = "__RequestVerificationToken";
    //    options.SuppressXFrameOptionsHeader = false;
    //});

    services.AddHttpClient();
    services.Configure<FormOptions>(x =>
    {
        x.ValueLengthLimit = int.MaxValue;
        x.MultipartBodyLengthLimit = int.MaxValue;
    });

    WebApplication app = builder.Build();

    //Important: this must be called before app.UseRouting()
    if ((kraftSettings.GeneralSettings.BlazorAreaAssembly.IsConfigured && kraftSettings.GeneralSettings.BlazorAreaAssembly.IsEnabled) ||
        (kraftSettings.GeneralSettings.RazorAreaAssembly.IsConfigured && kraftSettings.GeneralSettings.RazorAreaAssembly.IsEnabled))
    {
        #region localization
        //Set default language and provide other langs array
        var localizationOptions = new RequestLocalizationOptions()
            .SetDefaultCulture(kraftSettings.GeneralSettings.SupportedLanguages.Last())
            .AddSupportedCultures(kraftSettings.GeneralSettings.SupportedLanguages.ToArray())
            .AddSupportedUICultures(kraftSettings.GeneralSettings.SupportedLanguages.ToArray());

        //Set the language provider from cookie
        localizationOptions.RequestCultureProviders.Insert(0, new CookieRequestCultureProvider());

        app.UseRequestLocalization(localizationOptions);
        #endregion localization
    }

    app.UseBindKraft(app.Environment, restart =>
    {
        _RestartRequest = restart;
        if (restart)
        {
            KraftLogger.LogInformation("Restart: executed");
            Console.WriteLine($"========= Restarting App at: {DateTime.Now.ToLongTimeString()} =========");
        }
        _CancellationToken.Cancel();
    });

    if (app.Environment.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
        app.Lifetime.ApplicationStarted.Register(() =>
        {
            var server = app.Services.GetRequiredService<IServer>();
            LogAddresses(server.Features, app.Environment);
        });
    }
    else
    {
        app.UseExceptionHandler("/Home/Error");
    }

    app.UseStaticFiles();
    app.UseCookiePolicy();

    if (kraftSettings.GeneralSettings.MiddleWares.Count > 0)
    {
        foreach (var mw in kraftSettings.GeneralSettings.MiddleWares)
        {
            var implType = Type.GetType(mw.ImplementationAsString, true);
            var ifaceType = Type.GetType(mw.InterfaceAsString);
            if (implType.GetInterfaces().Contains(ifaceType))
            {
                var instance = Activator.CreateInstance(implType) as IUseMiddleWare;
                instance.Use(app);
            }
        }
    }

    app.UseRouting();
    app.UseAntiforgery();

    if (kraftSettings.GeneralSettings.RazorAreaAssembly.IsConfigured && kraftSettings.GeneralSettings.RazorAreaAssembly.IsEnabled)
    {
        app.MapDynamicControllerRoute<DynamicHostRouteTransformer>(
            kraftSettings.GeneralSettings.RazorAreaAssembly.DefaultRouting);

        foreach (var mapping in kraftSettings.GeneralSettings.RazorAreaAssembly.RouteMappings)
        {
            if (!string.IsNullOrEmpty(mapping.Pattern))
            {
                app.MapControllerRoute(
                    name: mapping.Name,
                    pattern: mapping.Pattern,
                    defaults: new { Controller = mapping.Controller, Action = mapping.Action }
                );
            }
        }
    }
    else if (kraftSettings.GeneralSettings.BlazorAreaAssembly.IsConfigured && kraftSettings.GeneralSettings.BlazorAreaAssembly.IsEnabled)
    {
        StartupBlazorDynamicAssembly blazorStartup = app.Services.GetRequiredService<StartupBlazorDynamicAssembly>();
        blazorStartup.Configure(app);
    }

    app.MapControllerRoute("acceptor", "redirect/{action=Index}/{id?}", new { controller = "Redirect" });
    app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
    app.MapControllerRoute("catchall", "/{**catchAll}", new { Controller = "Home", Action = "CatchAll" });

    await app.RunAsync(_CancellationToken.Token);

} while (_RestartRequest);


// --- SUPPORT METHODS ---
void ConfigureCookiePolicy(IServiceCollection services)
{
    services.Configure<CookiePolicyOptions>(options =>
    {
        options.Secure = Microsoft.AspNetCore.Http.CookieSecurePolicy.Always;
        options.CheckConsentNeeded = context => true;
        options.MinimumSameSitePolicy = SameSiteMode.None;
    });
}

void ConfigureApplicationParts(ApplicationPartManager apm, KraftGlobalConfigurationSettings kraftConfig)
{
    var razorContext = new RazorAssemblyLoadContext();
    foreach (var folder in kraftConfig.GeneralSettings.ModulesRootFolders)
    {
        var rootPath = Path.Combine(folder, "_PluginsReferences");
        foreach (var codeAssembly in kraftConfig.GeneralSettings.RazorAreaAssembly.AssemblyNamesCode)
        {
            var path = Path.Combine(rootPath, codeAssembly);
            if (File.Exists(path))
            {
                var asm = razorContext.LoadFromAssemblyPath(path);
                apm.ApplicationParts.Add(new AssemblyPart(asm));
                apm.ApplicationParts.Add(new CompiledRazorAssemblyPart(asm));
                foreach (var culture in kraftConfig.GeneralSettings.RazorAreaAssembly.SatelliteResourceLanguages)
                {
                    string satellitePath = Path.Combine(rootPath, culture, codeAssembly.Replace(".dll", ".resources.dll"));
                    if (File.Exists(satellitePath))
                    {
                        razorContext.LoadFromAssemblyPath(satellitePath);
                    }
                }
            }
            else
            {
                KraftLogger.LogCritical($"Missing assembly: {path}");
            }
        }

        foreach (var viewAssembly in kraftConfig.GeneralSettings.RazorAreaAssembly.AssemblyNamesView)
        {
            var path = Path.Combine(rootPath, viewAssembly);
            if (File.Exists(path))
            {
                var asm = razorContext.LoadFromAssemblyPath(path);
                apm.ApplicationParts.Add(new CompiledRazorAssemblyPart(asm));
            }
        }
    }
}

void LogAddresses(IFeatureCollection features, IWebHostEnvironment env)
{
    var addresses = features.Get<IServerAddressesFeature>();
    foreach (var address in addresses.Addresses)
    {
        Console.WriteLine($"Now listening on: {address}");
    }
    Console.WriteLine("Application started. Press Ctrl+C to shut down");
    Console.WriteLine($"Hosting environment: {(env.IsDevelopment() ? "Development" : "Production")}");
    Console.WriteLine($"Content root path: {env.ContentRootPath}");
}
