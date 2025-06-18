using System;

namespace QuickBooksProxy.Models;

public class QuickBooksSettings
    {
        public string ClientId { get; set; } = string.Empty;
        public string ClientSecret { get; set; } = string.Empty;
        public string RedirectUri { get; set; } = string.Empty;
        public string Environment { get; set; } = "sandbox"; // or "production"
        public string BaseUrl { get; set; } = "https://sandbox-quickbooks.api.intuit.com";
    }
