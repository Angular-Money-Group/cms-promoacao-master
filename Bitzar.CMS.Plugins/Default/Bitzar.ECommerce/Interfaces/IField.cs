using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Bitzar.ECommerce.Interfaces
{
    public interface IField 
    {
        string Field { get; set; }
        string Value { get; set; }
    }
}