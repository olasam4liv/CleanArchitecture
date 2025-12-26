using System;
using System.Collections.Generic;
using System.Text;

namespace SharedKernel.Helper.Interfaces;

public interface ISerializerService
{
    string Serialize<T>(T obj);

    string Serialize<T>(T obj, Type type);

    T Deserialize<T>(string text);
}
