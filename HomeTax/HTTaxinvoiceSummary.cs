﻿using System.Runtime.Serialization;


namespace Popbill.HomeTax
{
    public class HTTaxinvoiceSummary
    {
        [DataMember] public long? count;
        [DataMember] public long? supplyCostTotal;
        [DataMember] public long? taxTotal;
        [DataMember] public long? amountTotal;
    }
}