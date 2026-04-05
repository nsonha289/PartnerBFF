using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PartnerBFF.Application
{
    public static class AppConstant
    {
        public static readonly HashSet<string> ValidCurrencies = new HashSet<string>
        {
            "USD", "EUR", "GBP", "JPY", "AUD", "CAD", "CHF", "CNY", "VND" // add more as needed
        };
    }
}
