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
        public string BoM {get; set; } = string.Empty;

        public List<Item> ConsumedItems { get; } = new();

        public Pick(FishbowlSQL.Models.P) 
        {
            
        }

    }
 }
