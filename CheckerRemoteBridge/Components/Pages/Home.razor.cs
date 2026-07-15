// <copyright file="Home.razor.cs" company="Stanley Electric US Co. Inc.">
// Copyright (c) 2026 Stanley Electric US Co. Inc. Licensed under the MIT License.
// </copyright>

namespace CheckerRemoteBridge.Components.Pages;

using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Options;

using CheckerRemoteBridge.Models;
using CheckerRemoteBridge.Services;
using CheckerRemoteBridge.Options;

/// <summary>
/// Code-behind for the Home page.
/// </summary>
public sealed partial class Home : IDisposable
{
    private readonly CancellationTokenSource cts = new ();
    private readonly HashSet<int> launchedStationIds = [];
    private readonly SemaphoreSlim launchLock = new (1, 1);
    private bool startupEvaluationComplete = false;
    private bool modalOpen;
    private string modalTitle = string.Empty;
    private string modalBody = string.Empty;
    private string modalConfirmLabel = "Confirm";
    private string modalConfirmClass = string.Empty;
    private Func<Task>? pendingAction;

    [Inject]
    private CheckerStateStore StateStore { get; set; } = default!;

    [Inject]
    private CheckerActionService ActionService { get; set; } = default!;

    [Inject]
    private IOptions<CheckerFleetOptions> FleetOptions { get; set; } = default!;

    private int FinalCount => this.FleetOptions.Value.FinalCount;

    private string ServerDisplayName => this.FleetOptions.Value.ServerDisplayName;

    private int WatchdogStaleSeconds => this.FleetOptions.Value.WatchdogStaleSeconds;

    private int SelectedFinalId { get; set; } = 1;

    private DateTime Now { get; set; } = DateTime.Now; // Keeping this in the state synchronizes all comparisons to the current time.

    private string ClockText { get; set; } = string.Empty;

    /// <summary>
    /// When this component unloads, unsubscribe from the .
    /// </summary>
    public void Dispose()
    {
        this.cts.Cancel();
        this.cts.Dispose();

        this.StateStore.StatusChanged -= this.OnStatusChanged;

        // Handle the case where the page is disposed before OPC is fully set up.
        if (!this.startupEvaluationComplete)
        {
            this.StateStore.StatusChanged -= this.OnStatusChangedForStartup;
        }

        this.launchLock.Dispose();
        this.ActionService.EndServices();
    }

    /// <summary>
    /// When this page is initialized, bind the page update handler and auto-checksum+client launch handler.
    /// </summary>
    protected override void OnInitialized()
    {
        this.StateStore.StatusChanged += this.OnStatusChanged;
        this.StateStore.StatusChanged += this.OnStatusChangedForStartup;

        this.UpdateClock();
    }

    /// <summary>
    /// On the page's first render, set up the time manager/display and attempt to immediately complete the checksum verification/auto-launch.
    /// </summary>
    /// <param name="firstRender"><inheritdoc/></param>
    /// <returns>A Task representing the completion of this render.</returns>
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender)
        {
            return;
        }

        _ = this.RunClockLoop();
        await this.TryRunStartupEvaluationAsync();
    }

    /// <summary>
    /// Necessary method signature for subscribing to the OPC tag update. Simple wrapper for StateHasChanged().
    /// </summary>
    /// <param name="_">The discard operator, because the refresh is completed regardless of argument.</param>
    private void OnStatusChanged(int _) => this.InvokeAsync(this.StateHasChanged);

    /// <summary>
    /// Attempts to perform checksum and auto-launch on the final with ID = <paramref name="finalId"/>.
    /// </summary>
    /// <remarks>
    /// This method is simply a synchronous wrapper for <see cref="TryRunStartupEvaluationAsync"/>.
    /// </remarks>
    /// <param name="finalId">The final station number (1-based).</param>
    private void OnStatusChangedForStartup(int finalId)
    {
        if (this.startupEvaluationComplete)
        {
            return;
        }

        _ = Task.Run(async () => await this.TryRunStartupEvaluationAsync(finalId));
    }

    /// <summary>
    /// Attempts to perform checksum and auto-launch on the final with ID = <paramref name="finalId"/>.
    /// </summary>
    /// <param name="finalId">The final station number (1-based).</param>
    private async Task TryRunStartupEvaluationAsync(int? finalId = null)
    {
        if (this.startupEvaluationComplete)
        {
            return;
        }

        await this.launchLock.WaitAsync();
        try
        {
            IEnumerable<int> stationIds = finalId is null
                ? Enumerable.Range(1, this.FinalCount)
                : new[] { finalId.Value };

            foreach (int stationId in stationIds)
            {
                if (this.startupEvaluationComplete || this.launchedStationIds.Contains(stationId))
                {
                    continue;
                }

                CheckerStatus status = this.StateStore.Get(stationId);
                if (string.IsNullOrWhiteSpace(status.ExpectedChecksum) || !status.HasCheckerState)
                {
                    continue;
                }

                this.launchedStationIds.Add(stationId);
                await this.ActionService.CheckAndLaunchAsync(stationId);

                if (this.launchedStationIds.Count >= this.FinalCount)
                {
                    this.startupEvaluationComplete = true;
                    this.StateStore.StatusChanged -= this.OnStatusChangedForStartup;
                    System.Diagnostics.Debug.WriteLine("Successfully unsubscribed from startup launch handler.");
                    break;
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error launching station {finalId ?? 0}: {ex.Message}");
        }
        finally
        {
            this.launchLock.Release();
        }
    }

    /// <summary>
    /// Setter for <see cref="SelectedFinalId"/> for simple action binding.
    /// </summary>
    /// <param name="finalId">The final station number (1-based).</param>
    private void SelectFinal(int finalId) => this.SelectedFinalId = finalId;

    /// <summary>
    /// Opens a launch modal for the checker with ID = <paramref name="finalId"/>.
    /// </summary>
    /// <param name="finalId">The final station number (1-based).</param>
    private void ConfirmLaunch(int finalId) =>
        this.OpenModal(
            $"Auto-launch Final {finalId}",
            $"Connects to the Pi runnning checker {finalId} to run the checksum and checker launch script.",
            "Launch",
            "confirm-launch",
            async () => await this.ActionService.CheckAndLaunchAsync(finalId));

    /// <summary>
    /// Opens a reboot modal for the checker with ID = <paramref name="finalId"/>.
    /// </summary>
    /// <param name="finalId">The final station number (1-based).</param>
    private void ConfirmReboot(int finalId) =>
        this.OpenModal(
            $"Reboot Final {finalId}",
            "Creates a <strong>RebootRequest</strong> for the checker Pi to shut down and restart.",
            "Confirm reboot",
            "confirm-warn",
            async () => await this.ActionService.RequestRebootAsync(finalId));

    /// <summary>
    /// Opens a shutdown modal for the checker with ID = <paramref name="finalId"/>.
    /// </summary>
    /// <param name="finalId">The final station number (1-based).</param>
    private void ConfirmShutdown(int finalId) =>
        this.OpenModal(
            $"Shut down Final {finalId}",
            "Creates <strong>ShutdownRequest</strong> for the checker to shut down gracefully. THE PI CAN ONLY BE RESTARTED MANUALLY, AT THE LINE!",
            "Confirm shutdown",
            "confirm-danger",
            async () => await this.ActionService.RequestShutdownAsync(finalId));

    /// <summary>
    /// Opens a generic modal with a <paramref name="title"/>, <paramref name="body"/>, <paramref name="confirmLabel"/>, <paramref name="confirmClass"/>, and <paramref name="action"/> to take on confirmation.
    /// </summary>
    /// <param name="title">The modal title.</param>
    /// <param name="body">The body of the modal.</param>
    /// <param name="confirmLabel">The text for the confirmation button.</param>
    /// <param name="confirmClass">The color of the confirmation button.</param>
    /// <param name="action">The action to take upon confirmation.</param>
    private void OpenModal(string title, string body, string confirmLabel, string confirmClass, Func<Task> action)
    {
        this.modalTitle = title;
        this.modalBody = body;
        this.modalConfirmLabel = confirmLabel;
        this.modalConfirmClass = confirmClass;
        this.pendingAction = action;
        this.modalOpen = true;
    }

    /// <summary>
    /// Closes the modal.
    /// </summary>
    private void CloseModal()
    {
        this.modalOpen = false;
        this.pendingAction = null;
    }

    /// <summary>
    /// Executes <see cref="pendingAction"/>, then closes the modal.
    /// </summary>
    /// <returns>A Task representing that the modal action has been executed.</returns>
    private async Task ExecuteModalAction()
    {
        if (this.pendingAction is not null)
        {
            await this.pendingAction();
        }

        this.CloseModal();
    }

    /// <summary>
    /// Keeps the displayed clock in sync with the actual time.
    /// </summary>
    /// <returns>A Task representing that the clock has been disposed.</returns>
    private async Task RunClockLoop()
    {
        using PeriodicTimer timer = new (TimeSpan.FromSeconds(1));
        try
        {
            while (await timer.WaitForNextTickAsync())
            {
                this.UpdateClock();
                await this.InvokeAsync(this.StateHasChanged);
            }
        }
        catch (OperationCanceledException)
        {
            // This is the result of a cancellation canceling
        }
    }

    /// <summary>
    /// Updates the clock display to match the internal model.
    /// </summary>
    private void UpdateClock()
    {
        this.Now = DateTime.Now;
        this.ClockText = $"{this.Now:MMM d} {this.Now:HH:mm:ss}";
    }
}
