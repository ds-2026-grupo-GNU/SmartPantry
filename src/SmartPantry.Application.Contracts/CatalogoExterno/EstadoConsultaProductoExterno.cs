using System.Text.Json.Serialization;

namespace SmartPantry.CatalogoExterno;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum EstadoConsultaProductoExterno
{
    Encontrado,
    NoEncontrado,
    LimiteDeConsultas,
    ProveedorNoDisponible
}
