using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FishbowlSQL.Models;

namespace ZoneRV.Models.Inventory
{
    public class Item
    {
        public required string Name { get; set; } 

//TODO        public required int Stock { get; set; }

        public required string Category { get; set; }

        public required string Description { get; set; }

    }
}
