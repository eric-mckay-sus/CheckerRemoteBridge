// <copyright file="Program.cs" company="Stanley Electric US Co. Inc.">
// Copyright (c) 2026 Stanley Electric US Co. Inc. Licensed under the MIT License.
// </copyright>

namespace CheckerRemoteBridge;

using CheckerRemoteBridge.Components;
using CheckerRemoteBridge.Options;
using CheckerRemoteBridge.Services;
using OpcUtilities;

/// <summary>
/// Hosts the application startup and configuration.
/// </summary>
public static class Program
{
    /// <summary>
    /// Application entry point.
    /// </summary>
    /// <param name="args">Command-line arguments supplied by the host.</param>
    public static void Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
        builder.Services.AddBlazorBootstrap();

        builder.Services.Configure<CheckerFleetOptions>(
            builder.Configuration.GetSection(CheckerFleetOptions.SectionName));

        builder.Services.AddSingleton<CheckerStateStore>();
        builder.Services.AddSingleton<CheckerActionService>();
        builder.Services.AddSingleton<IPiControlService, PiControlServiceStub>();
        builder.Services.AddSingleton(CreateOpcClient);
        builder.Services.AddHostedService<OpcMonitorService>();

        builder.Services.AddRazorComponents()
            .AddInteractiveServerComponents();

        WebApplication app = builder.Build();

        if (!app.Environment.IsDevelopment())
        {
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();
        app.UseAntiforgery();

        app.MapRazorComponents<App>()
            .AddInteractiveServerRenderMode();

        app.Run();
    }

    private static IOpcClient CreateOpcClient(IServiceProvider serviceProvider) =>
        EasyUAOpcClient.TryCreateFromEnvironment(out EasyUAOpcClient? client) && client is not null
            ? client
            : new NullOpcClient();
}
