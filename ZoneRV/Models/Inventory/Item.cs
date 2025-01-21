using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoneRV.Models.Inventory
{
    public class Item
    {
        public string Nmae { get; set; } = string.Empty;

        public int Stock { get; set; }

        public string Category { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

    }
}
