﻿using CF.Customer.Domain.Services.Interfaces;
using Crypt = BCrypt.Net.BCrypt;

namespace CF.Customer.Domain.Services;

public class PasswordHasherService : IPasswordHasherService
{
    public string Hash(string password) => Crypt.HashPassword(password);

    public bool Verify(string password, string hash) => Crypt.Verify(password, hash);
}