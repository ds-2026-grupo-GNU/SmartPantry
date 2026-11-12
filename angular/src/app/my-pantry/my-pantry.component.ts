
import { Component, OnInit, ChangeDetectorRef, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { CoreModule, ListService, PagedResultDto } from '@abp/ng.core';
import { ThemeSharedModule, ConfirmationService, Confirmation } from '@abp/ng.theme.shared';

import { DespensaItemService } from '../proxy/despensa/despensa-item.service';
import { DespensaItemDto } from '../proxy/despensa/models';
import { EstadoItemDespensa } from '../proxy/despensa/estado-item-despensa.enum';

import { ProductoService } from '../proxy/productos/producto.service';
import { ProductoDto } from '../proxy/productos/models';

@Component({
  selector: 'app-my-pantry',
  standalone: true,
  imports: [CommonModule, CoreModule, ReactiveFormsModule, ThemeSharedModule],
  providers: [ListService],
  templateUrl: './my-pantry.component.html',
  styleUrl: './my-pantry.component.scss',
})
export class MyPantryComponent implements OnInit {
  public readonly list = inject(ListService);

  private readonly despensaItemService = inject(DespensaItemService);
  private readonly productoService = inject(ProductoService);
  private readonly cdr = inject(ChangeDetectorRef);
  private readonly fb = inject(FormBuilder);
  private readonly confirmation = inject(ConfirmationService);

  items = { items: [], totalCount: 0 } as PagedResultDto<DespensaItemDto>;
  isLoading = true;
  hasError = false;

  isModalOpen = false;
  form!: FormGroup;
  productos: ProductoDto[] = [];
  selectedItem: DespensaItemDto | null = null;
  estados = EstadoItemDespensa;

  ngOnInit(): void {
    this.buildForm();

    const streamCreator = (query: any) =>
      this.despensaItemService.getList(query);

    this.list.hookToQuery(streamCreator).subscribe({
      next: (response) => {
        this.items = response as PagedResultDto<DespensaItemDto>;
        this.isLoading = false;
        this.cdr.detectChanges();
      },
      error: () => {
        this.hasError = true;
        this.isLoading = false;
        this.cdr.detectChanges();
      },
    });

    this.list.get();
    this.loadProductos();
  }

  loadProductos(): void {
    this.productoService
      .getList({ maxResultCount: 1000, skipCount: 0 } as any)
      .subscribe({
        next: (res: any) => {
          console.log('Productos recibidos del servidor:', res.items);
          this.productos = res.items || [];
        },
        error: (err: any) => {
          console.error('Error al cargar los productos:', err);
        },
      });
  }

  loadItems(): void {
    this.isLoading = true;
    this.hasError = false;
    this.list.get();
  }

  createItem(): void {
    this.selectedItem = null;

    if (!this.form) {
      this.buildForm();
    } else {
      this.form.reset({
        productoId: null,
        cantidad: null,
        unidad: null,
        fechaVencimiento: null,
      });
    }

    this.isModalOpen = true;
  }

  buildForm(): void {
    this.form = this.fb.group({
      productoId: [null, Validators.required],
      cantidad: [null, [Validators.min(0)]],
      unidad: [null, Validators.maxLength(50)],
      fechaVencimiento: [null, Validators.required],
    });
  }

  save(): void {
    if (this.form.invalid) {
      return;
    }

    const request = this.selectedItem?.id
      ? this.despensaItemService.update(this.selectedItem.id, this.form.value)
      : this.despensaItemService.create(this.form.value);

    request.subscribe({
      next: () => {
        this.isModalOpen = false;
        this.form.reset();
        this.list.get();
      },
      error: (err: any) => {
        console.error('Error devuelto por el servidor:', err);
      },
    });
  }

  marcarComoConsumido(
    id: string | undefined,
    nombreProducto: string | null | undefined
  ): void {
    if (!id) {
      return;
    }

    const nombreSeguro = nombreProducto || 'este producto';

    this.confirmation
      .warn(
        `¿Estás seguro de que deseas marcar "${nombreSeguro}" como consumido?`,
        'Confirmar acción'
      )
      .subscribe((status: Confirmation.Status) => {
        if (status === Confirmation.Status.confirm) {
          this.despensaItemService.marcarComoConsumido(id).subscribe({
            next: () => {
              this.list.get();
            },
            error: (err: any) => {
              console.error('Error al marcar como consumido:', err);
            },
          });
        }
      });
  }

  deleteItem(
    id: string | undefined,
    nombreProducto: string | null | undefined
  ): void {
    if (!id) {
      return;
    }

    const nombreSeguro = nombreProducto || 'este producto';

    this.confirmation
      .warn(
        `¿Estás seguro de que deseas eliminar "${nombreSeguro}" de tu despensa?`,
        'Confirmar eliminación'
      )
      .subscribe((status: Confirmation.Status) => {
        if (status === Confirmation.Status.confirm) {
          this.despensaItemService.delete(id).subscribe(() => {
            this.list.get();
          });
        }
      });
  }

  editItem(item: DespensaItemDto): void {
    this.selectedItem = item;

    let fechaFormateada = null;

    if (item.fechaVencimiento) {
      fechaFormateada = new Date(item.fechaVencimiento)
        .toISOString()
        .split('T')[0];
    }

    this.form.patchValue({
      productoId: item.productoId,
      cantidad: item.cantidad,
      unidad: item.unidad,
      fechaVencimiento: fechaFormateada,
    });

    this.isModalOpen = true;
  }
}
