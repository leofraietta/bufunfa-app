using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Bufunfa.Api.Converters
{
    /// <summary>
    /// Conversor personalizado para DateTime em formato brasileiro
    /// </summary>
    public class BrazilianDateTimeConverter : JsonConverter<DateTime>
    {
        private const string DateFormat = "dd/MM/yyyy HH:mm:ss";
        private const string DateOnlyFormat = "dd/MM/yyyy";

        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var value = reader.GetString();
            if (string.IsNullOrEmpty(value))
                return default;

            // Tentar múltiplos formatos
            var formats = new[]
            {
                DateFormat,
                DateOnlyFormat,
                "yyyy-MM-ddTHH:mm:ss.fffZ", // ISO format
                "yyyy-MM-ddTHH:mm:ssZ",
                "yyyy-MM-dd HH:mm:ss",
                "yyyy-MM-dd"
            };

            foreach (var format in formats)
            {
                if (DateTime.TryParseExact(value, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out var result))
                {
                    return DateTime.SpecifyKind(result, DateTimeKind.Utc);
                }
            }

            // Fallback para parsing padrão
            if (DateTime.TryParse(value, out var fallback))
            {
                return DateTime.SpecifyKind(fallback, DateTimeKind.Utc);
            }

            throw new JsonException($"Não foi possível converter '{value}' para DateTime");
        }

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            // Converter para horário local brasileiro para exibição
            var brazilTimeZone = TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time");
            var localTime = TimeZoneInfo.ConvertTimeFromUtc(value, brazilTimeZone);
            
            writer.WriteStringValue(localTime.ToString(DateFormat, new CultureInfo("pt-BR")));
        }
    }

    /// <summary>
    /// Conversor para DateTime nullable
    /// </summary>
    public class BrazilianNullableDateTimeConverter : JsonConverter<DateTime?>
    {
        private readonly BrazilianDateTimeConverter _converter = new();

        public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
                return null;

            return _converter.Read(ref reader, typeof(DateTime), options);
        }

        public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options)
        {
            if (value == null)
            {
                writer.WriteNullValue();
            }
            else
            {
                _converter.Write(writer, value.Value, options);
            }
        }
    }
}
