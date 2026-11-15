import type { EntityDto } from '@abp/ng.core';
import type { EstadoItemDespensa } from './estado-item-despensa.enum';

export interface CreateDespensaItemDto {
  productoId: string;
  cantidad?: number | null;
  unidad?: string | null;
  fechaVencimiento?: string | null;
}

export interface DespensaItemDto extends EntityDto<string> {
  productoId?: string;
  nombreProducto?: string | null;
  cantidad?: number | null;
  unidad?: string | null;
  fechaVencimiento?: string | null;
  estado?: EstadoItemDespensa;
}

export interface UpdateDespensaItemDto {
  cantidad?: number | null;
  unidad?: string | null;
  fechaVencimiento?: string | null;
}
