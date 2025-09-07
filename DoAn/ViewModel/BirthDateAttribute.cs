using System;
using System.ComponentModel.DataAnnotations;

namespace DoAn.Validators
{
    public class BirthDateAttribute : ValidationAttribute
    {
        private readonly int _minAge;
        private readonly int _maxAge;

        public BirthDateAttribute(int minAge, int maxAge)
        {
            _minAge = minAge;
            _maxAge = maxAge;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null)
                return new ValidationResult("Ngày sinh không được để trống.");

            DateTime birthDate = (DateTime)value;
            DateTime today = DateTime.Today;

            if (birthDate > today)
                return new ValidationResult("Ngày sinh không được ở tương lai.");

            int age = today.Year - birthDate.Year;
            if (birthDate.Date > today.AddYears(-age)) age--;

            if (age < _minAge)
                return new ValidationResult($"Bạn chưa đủ {_minAge} tuổi để đăng ký, vui lòng nhờ người giám hộ.");

            if (age > _maxAge)
                return new ValidationResult("Ngày sinh không hợp lệ, vui lòng kiểm tra lại.");

            return ValidationResult.Success;
        }
    }
}
