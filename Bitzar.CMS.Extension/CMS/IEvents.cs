using System;
using Bitzar.CMS.Model;

namespace Bitzar.CMS.Extension.CMS
{
    public interface IEvents
    {
        void Trigger<T>(Enumerators.EventType eventType, T data, Exception exception = null);
        void Trigger<T>(string eventType, T data, Exception exception = null);
    }
}