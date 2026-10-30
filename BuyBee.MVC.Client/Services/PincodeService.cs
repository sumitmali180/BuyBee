using System.Text.RegularExpressions;

namespace EMart.Services
{
    public class PincodeService : IPincodeService
    {
        private readonly Dictionary<string, (string City, string State, bool IsMetro)> _knownPrefixes = new()
        {
            // Metro Hubs (Express Next-Day Delivery)
            { "400", ("Mumbai", "Maharashtra", true) },
            { "401", ("Thane / Palghar", "Maharashtra", true) },
            { "410", ("Navi Mumbai / Raigad", "Maharashtra", true) },
            { "411", ("Pune", "Maharashtra", true) },
            { "110", ("New Delhi", "Delhi", true) },
            { "122", ("Gurugram", "Haryana", true) },
            { "201", ("Noida / Ghaziabad", "Uttar Pradesh", true) },
            { "560", ("Bengaluru", "Karnataka", true) },
            { "500", ("Hyderabad", "Telangana", true) },
            { "600", ("Chennai", "Tamil Nadu", true) },
            { "700", ("Kolkata", "West Bengal", true) },
            { "380", ("Ahmedabad", "Gujarat", true) },

            // Major Tier-1 / Tier-2 Hubs
            { "302", ("Jaipur", "Rajasthan", false) },
            { "226", ("Lucknow", "Uttar Pradesh", false) },
            { "208", ("Kanpur", "Uttar Pradesh", false) },
            { "452", ("Indore", "Madhya Pradesh", false) },
            { "462", ("Bhopal", "Madhya Pradesh", false) },
            { "160", ("Chandigarh", "Punjab / Haryana", false) },
            { "141", ("Ludhiana", "Punjab", false) },
            { "143", ("Amritsar", "Punjab", false) },
            { "682", ("Kochi", "Kerala", false) },
            { "695", ("Thiruvananthapuram", "Kerala", false) },
            { "641", ("Coimbatore", "Tamil Nadu", false) },
            { "625", ("Madurai", "Tamil Nadu", false) },
            { "530", ("Visakhapatnam", "Andhra Pradesh", false) },
            { "520", ("Vijayawada", "Andhra Pradesh", false) },
            { "800", ("Patna", "Bihar", false) },
            { "834", ("Ranchi", "Jharkhand", false) },
            { "751", ("Bhubaneswar", "Odisha", false) },
            { "781", ("Guwahati", "Assam", false) },
            { "403", ("Panaji", "Goa", false) },
            { "395", ("Surat", "Gujarat", false) },
            { "390", ("Vadodara", "Gujarat", false) },
            { "440", ("Nagpur", "Maharashtra", false) },
            { "431", ("Chhatrapati Sambhaji Nagar", "Maharashtra", false) },
            { "422", ("Nashik", "Maharashtra", false) }
        };

        private readonly Dictionary<char, string> _zoneStates = new()
        {
            { '1', "Delhi & Northern Region" },
            { '2', "Uttar Pradesh & Uttarakhand" },
            { '3', "Rajasthan & Western Region" },
            { '4', "Maharashtra & Central Region" },
            { '5', "Andhra Pradesh, Telangana & Karnataka" },
            { '6', "Tamil Nadu & Kerala" },
            { '7', "West Bengal & Eastern Region" },
            { '8', "Bihar & Jharkhand" }
        };

        public PincodeDeliveryResult CheckDelivery(string? pincode)
        {
            if (string.IsNullOrWhiteSpace(pincode))
            {
                return new PincodeDeliveryResult
                {
                    IsValid = false,
                    Message = "Please enter your 6-digit postal PIN code."
                };
            }

            string cleanPin = pincode.Trim();

            if (!Regex.IsMatch(cleanPin, @"^[1-8][0-9]{5}$"))
            {
                return new PincodeDeliveryResult
                {
                    IsValid = false,
                    Pincode = cleanPin,
                    Message = "Invalid PIN code. Please enter a valid 6-digit Indian postal code."
                };
            }

            string prefix = cleanPin.Substring(0, 3);
            char zoneDigit = cleanPin[0];

            DateTime now = DateTime.Now;

            if (_knownPrefixes.TryGetValue(prefix, out var hub))
            {
                if (hub.IsMetro)
                {
                    DateTime deliveryDate = now.AddDays(1);
                    string formattedDate = deliveryDate.ToString("ddd, dd MMM");

                    return new PincodeDeliveryResult
                    {
                        IsValid = true,
                        Pincode = cleanPin,
                        City = hub.City,
                        State = hub.State,
                        EstimatedDelivery = $"Tomorrow, by 5:00 PM ({formattedDate})",
                        DeliveryTier = "Express",
                        IsExpressAvailable = true,
                        IsCodAvailable = true,
                        ShippingFee = 0,
                        Message = $"Express 24-hour delivery available to {hub.City}!"
                    };
                }
                else
                {
                    DateTime deliveryDate = now.AddDays(2);
                    string formattedDate = deliveryDate.ToString("ddd, dd MMM");

                    return new PincodeDeliveryResult
                    {
                        IsValid = true,
                        Pincode = cleanPin,
                        City = hub.City,
                        State = hub.State,
                        EstimatedDelivery = $"In 2 Days ({formattedDate})",
                        DeliveryTier = "Standard",
                        IsExpressAvailable = false,
                        IsCodAvailable = true,
                        ShippingFee = 0,
                        Message = $"Fast delivery available to {hub.City}, {hub.State}."
                    };
                }
            }

            // General zone fallback
            string regionName = _zoneStates.TryGetValue(zoneDigit, out var rName) ? rName : "India";
            DateTime standardDate = now.AddDays(4);

            return new PincodeDeliveryResult
            {
                IsValid = true,
                Pincode = cleanPin,
                City = regionName,
                State = "India",
                EstimatedDelivery = $"In 3 to 5 Days ({standardDate:ddd, dd MMM})",
                DeliveryTier = "Standard",
                IsExpressAvailable = false,
                IsCodAvailable = true,
                ShippingFee = 0,
                Message = $"Standard delivery & Cash on Delivery available to {cleanPin}."
            };
        }
    }
}
