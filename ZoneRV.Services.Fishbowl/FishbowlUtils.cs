using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using ZoneRV.Models.Inventory;

namespace ZoneRV.Services.Fishbowl
{
    internal static class FishbowlUtils
    {
        public static BoM ToBom(this FishbowlSQL.Models.BillOfMaterials fishbowlBom)
        {
            return new BoM()
            {
                Items = fishbowlBom.BomItems is null ? [] : fishbowlBom.BomItems.Select(x => x.Part.ToItem()).ToList(),
            };
        }

        public static Item ToItem(this FishbowlSQL.Models.Part fishbowlItem)
        {
            return new Item()
            {
                Name = fishbowlItem.Num,
                Category = fishbowlItem.Type.Name,
                Description = fishbowlItem.Description
            };
        }

        public static Pick ToPick(this FishbowlSQL.Models.Pick fishbowlPick)
        {
            return new Pick()
            {
                BoM = fishbowlPick.BoM,
                ConsumedItems = fishbowlPick.ConsumedItems
            };
        }
    }
}
