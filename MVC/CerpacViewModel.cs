// ==================== MODEL (Models/CerpacViewModel.cs) ====================



namespace BarcodeApi.MVC;

    public class CerpacViewModel
    {
        public string FullName { get; set; } = string.Empty;
        public string CountryInitial { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string MiddleName { get; set; } = string.Empty;
        public string Gender { get; set; } = string.Empty;
        public string DateOfBirth { get; set; } = string.Empty;
        public string Nationality { get; set; } = string.Empty;
       // public string PlaceOfBirth { get; set; } = string.Empty;
        public string PassportNumber { get; set; } = string.Empty;
        //public string NinNumber { get; set; } = string.Empty;
        //public string PhoneNumber { get; set; } = string.Empty;
        //public string ResidentialAddress { get; set; } = string.Empty;
        //public string Occupation { get; set; } = string.Empty;
        public string CerpacNumber { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string CompanyRCNumber { get; set; } = string.Empty;
        public string CompanyRegisteredAddress { get; set; } = string.Empty;
        public string CompanyTelephone { get; set; } = string.Empty;
        public string CompanyEmail { get; set; } = string.Empty;
        public string Designation { get; set; } = string.Empty;
       // public string ResidencePermitType { get; set; } = string.Empty;
        public string IssueDate { get; set; } = string.Empty;
        public string ExpiryDate { get; set; } = string.Empty;
        public string IssuingAuthority { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
       // public string BiometricCaptureDate { get; set; } = string.Empty;
        //public string LastRenewalDate { get; set; } = string.Empty;
       // public string ApplicationId { get; set; } = string.Empty;
       // public string Email { get; set; } = string.Empty;
        public string ProfileInitials { get; set; } = string.Empty;
       // public bool IsActive => Status?.ToUpper() == "ACTIVE";
       // public string StatusBadgeClass => IsActive ? "bg-green-100 text-green-700" : "bg-red-100 text-red-700";
    }