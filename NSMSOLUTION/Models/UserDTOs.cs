namespace NSMSOLUTION.Models
{
    public class UserDTOs
    {
        public class RegisterDto
        {
            public int CustomerID { get; set; }
            public string Username { get; set; }

            // Use Phone instead of Email
            public string PhoneNumber { get; set; }

            public string Password { get; set; }
        }
    }
}