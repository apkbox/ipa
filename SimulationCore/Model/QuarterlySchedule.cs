// --------------------------------------------------------------------------------
// <copyright file="QuarterlySchedule.cs" company="Alex Kozlov">
//   Copyright (c) Alex Kozlov. All rights reserved.
// </copyright>
// <summary>
//   Defines the QuarterlySchedule type.
// </summary>
// --------------------------------------------------------------------------------

namespace SimulationCore.Model
{
    using System;

    public class QuarterlySchedule : ISchedule
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
                this.nextDate = date.AddMonths(3);
                return true;
            }

            return false;
        }

        #endregion
    }
}