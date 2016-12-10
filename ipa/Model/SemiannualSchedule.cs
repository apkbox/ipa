// --------------------------------------------------------------------------------
// <copyright file="SemiannualSchedule.cs" company="Alex Kozlov">
//   Copyright (c) Alex Kozlov. All rights reserved.
// </copyright>
// <summary>
//   Defines the SemiannualSchedule type.
// </summary>
// --------------------------------------------------------------------------------

namespace Ipa.Model
{
    using System;

    public class SemiannualSchedule : ISchedule
    {
        #region Fields

        private DateTime? nextDate;

        #endregion

        #region Public Methods and Operators

        public bool IsArrived(DateTime date)
        {
            if (this.nextDate == null)
            {
                this.nextDate = date;
            }

            if (this.nextDate == date)
            {
                this.nextDate = date.AddMonths(6);
                return true;
            }

            return false;
        }

        #endregion
    }
}