using System.Net.Http.Json;
using MemberManagementSystem.Interfaces;
using MemberManagementSystem.Models;

namespace MemberManagementSystem.Dependencies;

/// <summary>
/// Retrieves the official TRM value from the government open data API.
/// </summary>
public class TrmService : ITrmService
{
    private readonly HttpClient _httpClient;

    public TrmService(HttpClient? httpClient = null)
    {
        _httpClient = httpClient ?? new HttpClient();
    }

    public async Task<TrmRate?> GetCurrentTrmAsync()
    {
        try
        {
            var url = "https://www.datos.gov.co/resource/32sa-8pi3.json?$order=vigenciadesde%20DESC&$limit=1";
            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var data = await response.Content.ReadFromJsonAsync<List<TrmApiResponse>>();
            var rate = data?.FirstOrDefault();

            if (rate is null)
            {
                return null;
            }

            return new TrmRate
            {
                VigenciaDesde = ParseDate(rate.vigenciadesde),
                VigenciaHasta = ParseDate(rate.vigenciahasta),
                Valor = ParseDecimal(rate.valor)
            };
        }
        catch
        {
            return null;
        }
    }

    private static DateTime? ParseDate(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return DateTime.TryParse(value, out var date) ? date : null;
    }

    private static decimal? ParseDecimal(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return decimal.TryParse(value, out var number) ? number : null;
    }

    private sealed class TrmApiResponse
    {
        public string? vigenciadesde { get; set; }
        public string? vigenciahasta { get; set; }
        public string? valor { get; set; }
    }
}
