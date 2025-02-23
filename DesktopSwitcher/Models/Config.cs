using System.Collections.Generic;
using System.Runtime.Serialization;
using Tomlyn.Model;

namespace DesktopSwitcher.Models
{
    public class Config : ITomlMetadataProvider
    {
        [IgnoreDataMember]
        public string Path { get; set; } = string.Empty;

        [DataMember(Name = "desktop_folder")]
        public List<DesktopFolder> DesktopFolders { get; init; } = [];

        // storage for comments and whitespace
        TomlPropertiesMetadata? ITomlMetadataProvider.PropertiesMetadata { get; set; }
    }
}
