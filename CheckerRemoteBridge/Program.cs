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
        builder.Services.AddSingleton<IPiControlService, FakePiControlService>(); // TODO swap FakePiControlService with PiControlService for deploy
        builder.Services.AddSingleton(CreateOpcClient);
        builder.Services.AddHostedService<OpcMonitorService>();

        builder.Services.AddRazorComponents()
            .AddInteractiveServerComponents();

        WebApplication app = builder.Build();

        IPiControlService piControlService = app.Services.GetRequiredService<IPiControlService>();

        // If the user has the environment variables for Pi connection, but connection failed, tell them.
        if (piControlService.IsConfigured && !await piControlService.InitializeAsync().ConfigureAwait(false))
        {
            throw new InvalidOperationException("SSH access denied to one or more devices. Please verify connection information, or clear SSH environment variables to run OPC only.");
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
