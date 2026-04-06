using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PartnerBFF.Infrastructure.Configurations
{
    public class ApiKeySettings
    {
        public Dictionary<string, string> Keys { get; set; } = new();
    }
}
