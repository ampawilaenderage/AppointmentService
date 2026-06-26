using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Contracts
{
    public class UserCreatedMessage
    {
        public int UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
    }
}
