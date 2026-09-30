using System;
using Shouldly;
using Xunit;

namespace SmartPantry.Productos;

// Fíjate que NO heredamos de SmartPantryDomainTestBase. 
// Esto evita que ABP intente conectarse a la base de datos u OpenIddict.
public class Producto_Tests
{
    [Fact]
    public void Deberia_Rechazar_Nombre_Vacio()
    {
        Assert.Throws<ArgumentException>(() => new Producto(Guid.NewGuid(), "   "));
    }

    [Fact]
    public void Deberia_Normalizar_Nombre_Usando_Trim()
    {
        var producto = new Producto(Guid.NewGuid(), "  Leche Descremada  ");
        producto.Nombre.ShouldBe("Leche Descremada");
    }

    [Fact]
    public void Deberia_Conservar_Normalizacion_Al_Modificar()
    {
        // Arrange: Empezamos con un producto válido
        var producto = new Producto(Guid.NewGuid(), "Fideos");

        // Act: Simulamos una actualización (modificación) con espacios extra
        producto.SetNombre("   Fideos Tirabuzon   ");

        // Assert: Verificamos que el Trim() actuó y lo normalizó (Requisito TP06)
        producto.Nombre.ShouldBe("Fideos Tirabuzon");
    }

    [Fact]
    public void Deberia_Rechazar_Modificacion_Invalida_Y_Mantener_Estado()
    {
        // Arrange: Empezamos con un producto válido
        var nombreOriginal = "Arroz";
        var producto = new Producto(Guid.NewGuid(), nombreOriginal);

        // Act & Assert 1: Verificamos que rechace el cambio inválido (vacío)
        Assert.Throws<ArgumentException>(() => producto.SetNombre("   "));

        // Assert 2: Verificamos que NO haya quedado un estado parcial/roto (Requisito TP06)
        producto.Nombre.ShouldBe(nombreOriginal);
    }
}