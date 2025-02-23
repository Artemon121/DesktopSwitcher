using System.Collections.Generic;
using Tomlyn.Model;

namespace DesktopSwitcher.Models
{
    public class DesktopFolder : ITomlMetadataProvider
    {
        public required string Name { get; set; }
        public required string Path { get; set; }
        
        public int IconSize { get; set; }
        
        // Base64 encoded binary data
        public string? IconLayouts { get; set; }

        // storage for comments and whitespace
        TomlPropertiesMetadata? ITomlMetadataProvider.PropertiesMetadata { get; set; }
    }
}
