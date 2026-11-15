import { mapEnumToOptions } from '@abp/ng.core';

export enum EstadoConsultaProductoExterno {
  Encontrado = 'Encontrado',
  NoEncontrado = 'NoEncontrado',
  LimiteDeConsultas = 'LimiteDeConsultas',
  ProveedorNoDisponible = 'ProveedorNoDisponible',
}

export const estadoConsultaProductoExternoOptions = mapEnumToOptions(EstadoConsultaProductoExterno);
