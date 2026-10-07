using System.Security.Cryptography;
using Kofi.Marketing.Domain.Coupons;

namespace Kofi.Marketing.Application;

public static class ReferralCode
{
    // No 0/O or 1/I, so a code read out loud is typed correctly.
    private const string Alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";

    public static CouponCode Generate() => new("REF-" + RandomNumberGenerator.GetString(Alphabet, 6));
}
