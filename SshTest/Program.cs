namespace SshTest;

using CheckerRemoteBridge.Services;

public static class Program
{
    private static readonly PiControlService service = new ();

    public static async Task Main()
    {
        bool initSuccess = await service.InitializeAsync();
        Console.WriteLine($"Service initialized: {initSuccess}");

        for(int i = 1; i<=5; i++)
        {
            // Need to connect first
            string? cmdResult = await service.RunChecksumScriptAsync(i);

            Console.WriteLine($"Final {i} online: checksum {cmdResult}");
        }
    }
}
