using System.Globalization;
using MemberManagementSystem.UI;

namespace MemberManagementSystem;

/// <summary>
/// Application entry point.
/// </summary>
internal static class Program
{
    private static async Task Main(string[] args)
    {
        var culture = new CultureInfo("es-CO");
        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;

        var menu = new ConsoleMenu();
        await menu.RunAsync();
    }
}
