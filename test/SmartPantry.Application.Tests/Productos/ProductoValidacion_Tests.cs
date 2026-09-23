using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Shouldly;
using Xunit;

namespace SmartPantry.Productos;

// Prueba pura, sin heredar de la base de ABP
public class ProductoValidacion_Tests
{
    [Fact]
    public void Deberia_Fallar_Validacion_Por_Faltar_Nombre()
    {
        // Arrange: Creamos un DTO con un nombre inválido/vacío
        var input = new CreateProductoDto { Nombre = "" };

        var contextoValidacion = new ValidationContext(input);
        var resultados = new List<ValidationResult>();

        // Act: Disparamos la validación nativa de las DataAnnotations (ej. [Required])
        bool esValido = Validator.TryValidateObject(input, contextoValidacion, resultados, true);

        // Assert: Verificamos que la validación falló
        esValido.ShouldBeFalse();
        resultados.ShouldNotBeEmpty();
    }
}