import type { CreateDespensaItemDto, DespensaItemDto, UpdateDespensaItemDto } from './models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto, PagedResultRequestDto } from '@abp/ng.core';
import { Injectable, inject } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class DespensaItemService {
  private restService = inject(RestService);
  apiName = 'Default';
  

  create = (input: CreateDespensaItemDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, DespensaItemDto>({
      method: 'POST',
      headers: { Accept: 'application/json' },
      url: '/api/app/despensa-item',
      body: input,
    },
    { apiName: this.apiName,...config });
  

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/app/despensa-item/${id}`,
    },
    { apiName: this.apiName,...config });
  

  getList = (input: PagedResultRequestDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<DespensaItemDto>>({
      method: 'GET',
      headers: { Accept: 'application/json' },
      url: '/api/app/despensa-item',
      params: { skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });
  

  marcarComoConsumido = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, DespensaItemDto>({
      method: 'POST',
      headers: { Accept: 'application/json' },
      url: `/api/app/despensa-item/${id}/marcar-consumido`,
    },
    { apiName: this.apiName,...config });
  

  update = (id: string, input: UpdateDespensaItemDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, DespensaItemDto>({
      method: 'PUT',
      headers: { Accept: 'application/json' },
      url: `/api/app/despensa-item/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });
}
