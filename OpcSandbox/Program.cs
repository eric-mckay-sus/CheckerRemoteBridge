namespace OpcSandbox;

using System.Reflection;
using OpcLabs.BaseLib.ComponentModel;
using OpcLabs.EasyOpc.UA;
using OpcLabs.EasyOpc.UA.Extensions;
using OpcLabs.EasyOpc.UA.OperationModel;

public static class OpcSandbox
{
    private static readonly UAEndpointDescriptor endpointDescriptor = ((UAEndpointDescriptor)"opc.tcp://SUS-KEPWARE-02.stanleyus.local:49320").WithUserNameIdentity("jot", "jot2024!");
    private static readonly EasyUAClient client = new ();

    public static void Main()
    {
        RegisterLicense();
        bool repeat = true;
        while (repeat)
        {
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

            EasyUAClientCore.SharedParameters.EngineParameters.CertificateAcceptancePolicy.AcceptAnyCertificate = true;

            try
            {
                Console.WriteLine("Reading OPC node value...");

                // Connection is implicitly opened and managed during the operation
                object value = IEasyUAClientExtension.ReadValue(client, endpointDescriptor, nodeId);

                Console.WriteLine($"Successfully read {nodeIdBuilder[3]} tag: {value}");
            }
            catch (UAException ex)
            {
                Console.WriteLine($"OPC Error: {ex.Message}");
            }

            Console.Write("Do you wish to read another tag? (y/n): ");
            char confirmation = (char)Console.ReadKey().Key;
            Console.WriteLine();
            repeat = confirmation.Equals('y') || confirmation.Equals('Y');
        }
    }

    private static void RegisterLicense()
    {
        try
            {
                LicensingManagement.Instance.RegisterManagedResource(
                    "QuickOPC",
                    "Multipurpose",
                    Assembly.GetExecutingAssembly(),
                    "OpcSandbox.license.bin"
                );

                Console.WriteLine("License applied successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to apply license: {ex.Message}");
            }
    }
}
