import type { ConsultarProductoExternoDto, ProductoExternoDto } from './models';
import { RestService, Rest } from '@abp/ng.core';
import { Injectable, inject } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class CatalogoExternoService {
  private restService = inject(RestService);
  apiName = 'Default';
  

  getPorCodigo = (input: ConsultarProductoExternoDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ProductoExternoDto>({
      method: 'GET',
      headers: { Accept: 'application/json' },
      url: '/api/app/catalogo-externo/por-codigo',
      params: { codigoBarras: input.codigoBarras },
    },
    { apiName: this.apiName,...config });
}