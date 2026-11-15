import type { EstadoConsultaProductoExterno } from './estado-consulta-producto-externo.enum';

export interface ConsultarProductoExternoDto {
  codigoBarras: string;
}

export interface ProductoExternoDto {
  estado?: EstadoConsultaProductoExterno;
  nombre?: string | null;
  marca?: string | null;
  imagenUrl?: string | null;
}
