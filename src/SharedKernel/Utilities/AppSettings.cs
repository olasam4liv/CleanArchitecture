using System;
using System.Collections.Generic;
using System.Text;

namespace SharedKernel.Helper;

public class AppSettings
{
    public bool AddBufferSize { get; set; }
    public bool EncryptResponseData { get; set; }
    public int EncryptResponseDataBufferSize { get; set; }
    public bool IsLive { get; set; }
    public bool UseEncryptedData { get; set; }
}
