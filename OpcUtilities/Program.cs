// <copyright file="Program.cs" company="Stanley Electric US Co. Inc.">
// Copyright (c) 2026 Stanley Electric US Co. Inc. Licensed under the MIT License.
// </copyright>

namespace OpcUtilities;

/// <summary>
/// Interactive console utilities for interfacing with OPC software using the EasyOpc SDK from OPC Labs.
/// </summary>
public static class OpcUtilities
{
    /// <summary>
    /// Entry point for the program. Builds a node ID from its constituent parts, then performs the desired read/write.
    /// </summary>
    /// <returns>A Task representing that the program is complete.</returns>
    public static async Task Main()
    {
        if (!EasyUAOpcClient.TryCreateFromEnvironment(out EasyUAOpcClient? client) || client is null)
        {
            Console.WriteLine("Required environment variables OPC_URI, OPC_USER, and OPC_PASS are missing.");
            return;
        }

        using (client)
        {
            bool repeat = true;
            while (repeat)
            {
                Console.Write("Please indicate whether you would like to read, write, or monitor the target tag: ");
                string response = Console.ReadLine() ?? string.Empty;
                bool isRead = response.Equals("read", StringComparison.OrdinalIgnoreCase);
                bool isWrite = response.Equals("write", StringComparison.OrdinalIgnoreCase);

                string?[] nodeIdBuilder = new string?[4];
                Console.Write("Please enter channel name: ");
                nodeIdBuilder[0] = Console.ReadLine();
                Console.Write("Please enter device name: ");
                nodeIdBuilder[1] = Console.ReadLine();
                Console.Write("Please enter tag group (if applicable): ");
                nodeIdBuilder[2] = Console.ReadLine();
                Console.Write("Please enter tag name: ");
                nodeIdBuilder[3] = Console.ReadLine();

                string nodeId = "ns=2;s=" + string.Join('.', nodeIdBuilder.Where(s => !string.IsNullOrEmpty(s)));

                if (isRead)
                {
                    await ReadOpcTag(client, nodeId);
                }
                else if (isWrite)
                {
                    object? toWrite = null;
                    while (toWrite is null)
                    {
                        Console.Write("Please enter the desired value for this tag: ");
                        toWrite = Console.ReadLine();
                    }

                    await WriteOpcTag(client, nodeId, toWrite);
                }
                else
                {
                    MonitorOpcTag(client, nodeId);
                }

                Console.Write("Do you wish to process another tag? (y/n): ");
                char confirmation = Console.ReadKey().KeyChar;
                Console.WriteLine();
                repeat = confirmation.Equals('y') || confirmation.Equals('Y');
            }
        }
    }

    private static async Task ReadOpcTag(IOpcClient client, string nodeId)
    {
        Console.WriteLine("Reading OPC node value...");
        object? value = await client.ReadAsync(nodeId);
        if (value is null)
        {
            Console.WriteLine($"Failed to read {nodeId.Split('.')[^1]} tag.");
            return;
        }

        Console.WriteLine($"Successfully read {nodeId.Split('.')[^1]} tag: {value}");
    }

    private static async Task WriteOpcTag(IOpcClient client, string nodeId, object value)
    {
        Console.WriteLine("Writing OPC node value...");
        bool success = await client.WriteAsync(nodeId, value);
        if (success)
        {
            Console.WriteLine($"Successfully wrote {value} to {nodeId.Split('.')[^1]} tag");
            return;
        }

        Console.WriteLine($"Failed to write {value} to {nodeId.Split('.')[^1]} tag");
    }

    private static void MonitorOpcTag(IOpcClient client, string nodeId)
    {
        Console.WriteLine("Initializing monitor for OPC node...");

        client.SubscribeDataChange(
            nodeId,
            1000,
            (_, value) => Console.WriteLine($"\n\tTag {nodeId.Split('.')[^1]} changed to: {value}"));

        Console.WriteLine($"Monitor initialized. All changes to {nodeId.Split('.')[^1]} will be reported here.");
        Console.WriteLine("Press any key to stop monitoring...");
        Console.ReadKey(intercept: true);
        client.UnsubscribeAll();
    }
}
