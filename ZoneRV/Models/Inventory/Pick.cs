using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FishbowlSQL.Models;

namespace ZoneRV.Models.Inventory
{
    public class Pick
    {
        public required string BoM {get; set; }

        public required List<Item> ConsumedItems { get; set; }



    }
 }
