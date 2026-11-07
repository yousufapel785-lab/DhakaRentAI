using System;
using System.Collections.Generic;
using System.Text;

namespace DhakaRent.Entities.Enums
{
    public enum ViewingRequestStatus
    {
        Pending = 1,
        Accepted = 2,
        Rejected = 3,
        RescheduleRequested = 4,
        Cancelled = 5,
        Completed = 6
    }
}