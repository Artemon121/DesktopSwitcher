using Microsoft.Win32;

namespace DesktopSwitcher.Models
{
    public class RegistryKeyValue
    {
        public required string KeyPath { get; set; }
        public required string ValueName { get; set; }
        public required object? Value { get; set; }
        public required RegistryValueKind ValueKind { get; set; }
    }
}
