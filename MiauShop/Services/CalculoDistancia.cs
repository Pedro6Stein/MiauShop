using MiauShop.Models;

namespace MiauShop.Services;

// Distância entre dois CEPs a partir das coordenadas, sem serviço pago de rotas.
public static class CalculoDistancia
{
    private const double RaioDaTerraKm = 6371;

    // A estrada costuma ser ~30% mais longa que a linha reta (curvas, contornos).
    public const double FatorEstrada = 1.3;

    // Um dia de transporte a cada 500 km (mínimo 1).
    public const int KmPorDiaDeTransporte = 500;

    // Fórmula de Haversine: distância em linha reta sobre a superfície da Terra.
    public static double LinhaRetaKm(Coordenada a, Coordenada b)
    {
        static double Radianos(double graus) => graus * Math.PI / 180;

        var dLat = Radianos(b.Latitude - a.Latitude);
        var dLon = Radianos(b.Longitude - a.Longitude);
        var h = Math.Pow(Math.Sin(dLat / 2), 2)
              + Math.Cos(Radianos(a.Latitude)) * Math.Cos(Radianos(b.Latitude)) * Math.Pow(Math.Sin(dLon / 2), 2);

        return 2 * RaioDaTerraKm * Math.Asin(Math.Sqrt(h));
    }

    public static int EstradaKm(Coordenada origem, Coordenada destino) =>
        Math.Max(1, (int)Math.Round(LinhaRetaKm(origem, destino) * FatorEstrada));

    public static int DiasDeTransporte(int distanciaKm) =>
        1 + distanciaKm / KmPorDiaDeTransporte;
}
