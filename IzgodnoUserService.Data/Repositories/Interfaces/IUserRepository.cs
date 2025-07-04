﻿using IzgodnoUserService.Data.Models.UserEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IzgodnoUserService.Data.Repositories.Interfaces
{
    public interface IUserRepository : IGenericRepository<AppUser>
    {
        Task<AppUser?> GetByEmailAsync(string email);
    }
}
