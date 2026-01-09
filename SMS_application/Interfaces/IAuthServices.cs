using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities;

namespace Application.Services
{
    public interface IAuthServices
    {
       Task<bool> Register(string email, string password);
        //bool Register(string email, string password);
        LoginResponse Login(string email, string password);
        Task SendActivationEmail(string toEmail, string activationLink);
        Task SendResetPasswordEmail(string toEmail, string resetLink);
    }
}
