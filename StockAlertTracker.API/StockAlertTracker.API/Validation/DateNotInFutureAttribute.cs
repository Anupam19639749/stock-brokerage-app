using System.ComponentModel.DataAnnotations;

namespace StockAlertTracker.API.Validation
{
    public class DateNotInFutureAttribute : ValidationAttribute
    {
        public DateNotInFutureAttribute()
        {
            ErrorMessage = "Date of Birth cannot be in the future.";
        }

        public override bool IsValid(object value)
        {
            // This validation is for a nullable DateTime (DateTime?)
            if (value == null)
            {
                return true; // Null is considered valid (it's optional)
            }

            if (value is DateTime dateTime)
            {
                // We compare only the Date part, ignoring the time
                return dateTime.Date <= DateTime.UtcNow.Date;
            }

            return false;
        }
    }
}