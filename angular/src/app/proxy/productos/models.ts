import type { EntityDto } from '@abp/ng.core';

export interface CreateProductoDto {
  nombre: string;
}

export interface ProductoDto extends EntityDto<string> {
  nombre?: string;
}

export interface UpdateProductoDto {
  nombre: string;
}
