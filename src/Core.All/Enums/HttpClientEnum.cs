using System.ComponentModel;

namespace Core.All.Enums;

public enum HttpClientEnum
{
    [Description("GitHub")]
    GitHub,

    [Description("Upload")]
    Upload,

    [Description("AuthUpload")]
    AuthUpload,
}
