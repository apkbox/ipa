namespace Ipa.Model
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class TradeOrder
    {
        public decimal? BuyPrice { get; set; }

        public FinSec Security { get; set; }

        public decimal? SellPrice { get; set; }

        public decimal Units { get; set; }
    }
}
