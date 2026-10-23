using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace MiauShop.Services;

// Transforma um nome em trecho de URL: "Comidinhas & Petiscos" -> "comidinhas-e-petiscos".
public static partial class GeradorSlug
{
    public static string Gerar(string texto)
    {
        // Separa letra e acento (FormD) e descarta os acentos.
        var semAcento = new string(texto.Normalize(NormalizationForm.FormD)
            .Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
            .ToArray());

        var minusculo = semAcento.ToLowerInvariant().Replace("&", " e ");
        return ForaDoPadrao().Replace(minusculo, "-").Trim('-');
    }

    // Se o slug já existe, acrescenta -2, -3...
    public static string Unico(string texto, IEnumerable<string> existentes)
    {
        var usados = existentes.ToHashSet();
        var baseSlug = Gerar(texto);
        var slug = baseSlug;

        for (var numero = 2; usados.Contains(slug); numero++)
            slug = $"{baseSlug}-{numero}";

        return slug;
    }

    [GeneratedRegex("[^a-z0-9]+")]
    private static partial Regex ForaDoPadrao();
}
