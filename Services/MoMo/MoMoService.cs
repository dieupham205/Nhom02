using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace ToursAndTravelsManagement.Services.MoMo;

public class MoMoService
{
    private readonly IConfiguration _configuration;
    private readonly IHttpClientFactory _httpClientFactory;

    public MoMoService(
        IConfiguration configuration,
        IHttpClientFactory httpClientFactory)
    {
        _configuration = configuration;
        _httpClientFactory = httpClientFactory;
    }

    private string Sign(
        string rawData,
        string secretKey)
    {
        using var hmac =
            new HMACSHA256(
                Encoding.UTF8.GetBytes(secretKey));

        var hash =
            hmac.ComputeHash(
                Encoding.UTF8.GetBytes(rawData));

        return BitConverter
            .ToString(hash)
            .Replace("-", "")
            .ToLower();
    }

    public async Task<string?> CreatePaymentAsync(
        int bookingId,
        decimal amount)
    {
        var partnerCode =
            _configuration["MoMo:PartnerCode"];

        var accessKey =
            _configuration["MoMo:AccessKey"];

        var secretKey =
            _configuration["MoMo:SecretKey"];

        var endpoint =
            _configuration["MoMo:Endpoint"];

        var returnUrl =
            _configuration["MoMo:ReturnUrl"];

        var notifyUrl =
            _configuration["MoMo:NotifyUrl"];

        // Gắn bookingId vào orderId để callback xử lý dễ hơn
        var orderId =
            $"BOOKING_{bookingId}_{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}";

        var requestId =
            Guid.NewGuid().ToString();

        var orderInfo =
            $"Thanh toan booking {bookingId}";

        var rawHash =
            $"accessKey={accessKey}" +
            $"&amount={(long)amount}" +
            $"&extraData=" +
            $"&ipnUrl={notifyUrl}" +
            $"&orderId={orderId}" +
            $"&orderInfo={orderInfo}" +
            $"&partnerCode={partnerCode}" +
            $"&redirectUrl={returnUrl}" +
            $"&requestId={requestId}" +
            $"&requestType=captureWallet";

        var signature =
            Sign(rawHash, secretKey!);

        var requestBody = new
        {
            partnerCode,
            requestId,
            amount = ((long)amount).ToString(),
            orderId,
            orderInfo,
            redirectUrl = returnUrl,
            ipnUrl = notifyUrl,
            requestType = "captureWallet",
            extraData = "",
            lang = "vi",
            signature
        };

        var client =
            _httpClientFactory.CreateClient();

        var content =
            new StringContent(
                JsonConvert.SerializeObject(requestBody),
                Encoding.UTF8,
                "application/json");

        var response =
            await client.PostAsync(
                endpoint,
                content);

        var json =
            await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception(json);
        }

        dynamic result =
            JsonConvert.DeserializeObject(json)!;

        // Debug
        Console.WriteLine(json);

        return result.payUrl;
    }
}