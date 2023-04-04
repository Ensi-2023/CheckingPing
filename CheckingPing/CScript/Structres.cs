using System;

namespace CheckingPing.CScript
{
    public class Structres
    {
        public struct Packet
        {
            public double CountPacketSuccess;
            public double CountPacketFailure;

            /// <summary>
            /// 
            /// </summary>
            /// <returns>return *.** %</returns>
            public string PercentTimeOutPacket()
            {
                if (CountPacketFailure == 0)
                    return "0 %";

                if (CountPacketSuccess == 0 && CountPacketFailure > 0)
                    return "100 %";

                return $"{Math.Round(Formula(CountPacketSuccess, CountPacketFailure), 2)} %";
            }

            private double Formula(double field1, double field2)
            {
                return (field2 / (field1 + field2)) * 100;
            }
        }    
    }
}
