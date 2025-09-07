using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Bufunfa.Api.Converters
{
    /// <summary>
    /// Conversor para garantir que todos os DateTime sejam salvos como UTC no PostgreSQL
    /// </summary>
    public class UtcDateTimeConverter : ValueConverter<DateTime, DateTime>
    {
        public UtcDateTimeConverter() : base(
            // Converter para UTC ao salvar no banco
            dateTime => dateTime.Kind == DateTimeKind.Utc 
                ? dateTime 
                : DateTime.SpecifyKind(dateTime, DateTimeKind.Utc),
            
            // Manter como UTC ao ler do banco
            dateTime => DateTime.SpecifyKind(dateTime, DateTimeKind.Utc))
        {
        }
    }

    /// <summary>
    /// Conversor para DateTime nullable
    /// </summary>
    public class UtcNullableDateTimeConverter : ValueConverter<DateTime?, DateTime?>
    {
        public UtcNullableDateTimeConverter() : base(
            // Converter para UTC ao salvar no banco
            dateTime => dateTime.HasValue 
                ? (dateTime.Value.Kind == DateTimeKind.Utc 
                    ? dateTime.Value 
                    : DateTime.SpecifyKind(dateTime.Value, DateTimeKind.Utc))
                : (DateTime?)null,
            
            // Manter como UTC ao ler do banco
            dateTime => dateTime.HasValue 
                ? DateTime.SpecifyKind(dateTime.Value, DateTimeKind.Utc)
                : (DateTime?)null)
        {
        }
    }
}
