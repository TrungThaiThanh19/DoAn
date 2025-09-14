using System;
using System.ComponentModel.DataAnnotations;

namespace DoAn.ViewModels
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class BirthDateAttribute : ValidationAttribute
    {
        public int MinAge { get; }
        public int MaxAge { get; }

        public BirthDateAttribute(int minAge, int maxAge)
        {
            MinAge = minAge;
            MaxAge = maxAge;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            // Để [Required] xử lý null/empty
            if (value is not DateTime dob) return ValidationResult.Success;

            var today = DateTime.Today;
            if (dob > today)
                return new ValidationResult(ErrorMessage ?? "Ngày sinh không được ở tương lai.");

            var age = today.Year - dob.Year;
            if (dob.Date > today.AddYears(-age)) age--;

            if (age < MinAge || age > MaxAge)
                return new ValidationResult(ErrorMessage ?? $"Tuổi phải trong khoảng {MinAge}–{MaxAge}.");

            return ValidationResult.Success;
        }
    }
}
