using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities;

namespace Application.Services
{
    public class LoginResponse
    {
        public bool Success { get; set; }
        public bool IsActive { get; set; }
        public string Message { get; set; }
        public Banda? User { get; set; }
    }
}
