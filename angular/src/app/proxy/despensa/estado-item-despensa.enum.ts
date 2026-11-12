import { mapEnumToOptions } from '@abp/ng.core';

export enum EstadoItemDespensa {
  Disponible = 1,
  Consumido = 2,
  Desechado = 3,
}

export const estadoItemDespensaOptions = mapEnumToOptions(EstadoItemDespensa);
