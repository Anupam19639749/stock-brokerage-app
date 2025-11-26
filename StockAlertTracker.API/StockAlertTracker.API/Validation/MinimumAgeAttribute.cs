using System.ComponentModel.DataAnnotations;

namespace StockAlertTracker.API.Validation
{
    public class MinimumAgeAttribute : ValidationAttribute
    {
        private readonly int _minimumAge;

        public MinimumAgeAttribute(int minimumAge)
        {
            _minimumAge = minimumAge;
            ErrorMessage = $"You must be at least {_minimumAge} years old.";
        }

        public override bool IsValid(object value)
        {
            // Optional field: if they don't provide a DOB, it's valid.
            if (value == null)
            {
                return true;
            }

            if (value is DateTime dob)
            {
                var today = DateTime.UtcNow.Date;
                var age = today.Year - dob.Year;

                // This is the crucial part:
                // We check if they've already had their birthday this year.
                // If their birthday is *after* today's date, we subtract one year from their age.
                if (dob.Date > today.AddYears(-age))
                {
                    age--;
                }

                return age >= _minimumAge;
            }

            return false;
        }
    }
}