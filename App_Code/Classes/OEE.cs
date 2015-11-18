using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DowntimeCollection_Demo.Classes
{
    public class OEE
    {
        protected DateTime from { get; set; }
        protected DateTime to { get; set; }

        public decimal machineAvailability { get; set; }

        public decimal downtimeMinutes { get; set; }

        public decimal uptime { get; set; }

        public decimal scheduledTimeMinutes { get; set; }

        public decimal performance { get; set; }

        public decimal partsPerMinute { get; set; }
        public decimal partsPerHour { get; set; }
        public int skuId { get; set; }
        public decimal totalPieces { get; set; }

        public decimal quality { get; set; }
        public decimal goodUnits { get; set; }

        public decimal total { get; set; }

        public OEE()
        {
        }

        public void setFrom(DateTime from)
        {
            this.from = from;
        }

        public DateTime getFrom()
        {
            return from;
        }

        public void setTo(DateTime to)
        {
            this.to = to;
        }

        public DateTime getTo()
        {
            return to;
        }

        public decimal calculateTotal()
        {
            calculateMachineAvailability();
            calculatePerformance();
            calculateQuality();

            total = machineAvailability * performance * quality;

            return total;
        }

        public decimal calculateMachineAvailability()
        {
            scheduledTimeMinutes = Convert.ToDecimal(to.Subtract(from).TotalMinutes);

            uptime = scheduledTimeMinutes - downtimeMinutes;

            machineAvailability = uptime / scheduledTimeMinutes;

            return machineAvailability;
        }

        public decimal calculatePerformance()
        {
            partsPerMinute = partsPerHour / 60;

            performance = totalPieces / (partsPerHour * Convert.ToDecimal(to.Subtract(from).TotalHours));

            return performance;
        }

        public decimal calculateQuality()
        {
            if (totalPieces != 0)
                quality = goodUnits / totalPieces;
            else
                quality = 0;

            return quality;
        }
    }
}