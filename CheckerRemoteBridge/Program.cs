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
    /// <returns>A Task representing that the app is running.</returns>
    public static async Task Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
        builder.Services.AddBlazorBootstrap();

        builder.Services.Configure<CheckerFleetOptions>(
            builder.Configuration.GetSection(CheckerFleetOptions.SectionName));

        builder.Services.AddSingleton<CheckerStateStore>();
        builder.Services.AddSingleton<CheckerActionService>();
        builder.Services.AddSingleton<IPiControlService, PiControlService>();
        builder.Services.AddSingleton(CreateOpcClient);
        builder.Services.AddHostedService<OpcMonitorService>();

        builder.Services.AddRazorComponents()
            .AddInteractiveServerComponents();

        WebApplication app = builder.Build();

        IPiControlService piControlService = app.Services.GetRequiredService<IPiControlService>();
        if (!await piControlService.InitializeAsync().ConfigureAwait(false))
        {
            throw new InvalidOperationException("SSH access to all checker Pi devices is required before application startup.");
        }

        if (!app.Environment.IsDevelopment())
        {
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();
        app.UseAntiforgery();

        app.MapRazorComponents<App>()
            .AddInteractiveServerRenderMode();

        await app.RunAsync();
    }

    private static IOpcClient CreateOpcClient(IServiceProvider serviceProvider) =>
        EasyUAOpcClient.TryCreateFromEnvironment(out EasyUAOpcClient? client) && client is not null
            ? client
            : new NullOpcClient();
}
