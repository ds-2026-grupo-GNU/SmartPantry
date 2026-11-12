using Riok.Mapperly.Abstractions;
using Volo.Abp.Mapperly;
using SmartPantry.Authors;
using SmartPantry.Books;
using SmartPantry.Productos;

namespace SmartPantry;

// ==========================================
// 1. TUS MAPEOS DE PRODUCTO (Despensa)
// ==========================================
[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class SmartPantryProductoToProductoDtoMapper : MapperBase<Producto, ProductoDto>
{
    public override partial ProductoDto Map(Producto source);
    public override partial void Map(Producto source, ProductoDto destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class SmartPantryCreateProductoDtoToProductoMapper : MapperBase<CreateProductoDto, Producto>
{
    public override partial Producto Map(CreateProductoDto source);
    public override partial void Map(CreateProductoDto source, Producto destination);
}

// ==========================================
// 2. MAPEOS DE LA PLANTILLA RESTAURADOS Y CORREGIDOS
// (Usamos 'None' para que ignoren CreationTime, CreatorId, etc.)
// ==========================================
[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class SmartPantryBookToBookDtoMapper : MapperBase<Book, BookDto>
{
    [MapperIgnoreTarget(nameof(BookDto.AuthorName))]
    public override partial BookDto Map(Book source);

    [MapperIgnoreTarget(nameof(BookDto.AuthorName))]
    public override partial void Map(Book source, BookDto destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class SmartPantryCreateUpdateBookDtoToBookMapper : MapperBase<CreateUpdateBookDto, Book>
{
    public override partial Book Map(CreateUpdateBookDto source);
    public override partial void Map(CreateUpdateBookDto source, Book destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class SmartPantryAuthorToAuthorDtoMapper : MapperBase<Author, AuthorDto>
{
    public override partial AuthorDto Map(Author source);
    public override partial void Map(Author source, AuthorDto destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class SmartPantryCreateUpdateAuthorDtoToAuthorMapper : MapperBase<CreateUpdateAuthorDto, Author>
{
    public override partial Author Map(CreateUpdateAuthorDto source);
    public override partial void Map(CreateUpdateAuthorDto source, Author destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class SmartPantryAuthorToAuthorExcelDtoMapper : MapperBase<Author, AuthorExcelDto>
{
    public override partial AuthorExcelDto Map(Author source);
    public override partial void Map(Author source, AuthorExcelDto destination);
}