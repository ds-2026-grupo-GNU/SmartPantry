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
}