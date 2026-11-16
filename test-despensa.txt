import { ComponentFixture, TestBed } from '@angular/core/testing';
import { NO_ERRORS_SCHEMA } from '@angular/core'; // IMPORT CRÍTICO
import { MyPantryComponent } from './my-pantry.component';
import { DespensaItemService } from '../proxy/despensa/despensa-item.service';
import { ProductoService } from '../proxy/productos/producto.service';
import { ConfirmationService, Confirmation } from '@abp/ng.theme.shared';
import { ListService } from '@abp/ng.core';
import { ReactiveFormsModule } from '@angular/forms';
import { of, throwError } from 'rxjs';
import { EstadoItemDespensa } from '../proxy/despensa/estado-item-despensa.enum';
import { CommonModule } from '@angular/common';
import { Pipe, PipeTransform } from '@angular/core';

// Pipe mockeado para evitar el error NG0302
@Pipe({
  name: 'abpLocalization',
  standalone: true
})
class MockAbpLocalizationPipe implements PipeTransform {
  transform(value: string): string {
    return value;
  }
}

describe('MyPantryComponent', () => {
  let component: MyPantryComponent;
  let fixture: ComponentFixture<MyPantryComponent>;

  let mockDespensaService: any;
  let mockProductoService: any;
  let mockConfirmationService: any;
  let mockListService: any;

  beforeEach(async () => {
    mockDespensaService = {
      getList: jasmine.createSpy('getList').and.returnValue(of({ items: [], totalCount: 0 })),
      create: jasmine.createSpy('create').and.returnValue(of({})),
      update: jasmine.createSpy('update').and.returnValue(of({})),
      delete: jasmine.createSpy('delete').and.returnValue(of({})),
      marcarComoConsumido: jasmine.createSpy('marcarComoConsumido').and.returnValue(of({}))
    };

    mockProductoService = {
      getList: jasmine.createSpy('getList').and.returnValue(of({ items: [], totalCount: 0 }))
    };

    mockConfirmationService = {
      warn: jasmine.createSpy('warn').and.returnValue(of(Confirmation.Status.confirm))
    };

    mockListService = {
      get: jasmine.createSpy('get'),
      hookToQuery: jasmine.createSpy('hookToQuery').and.callFake((streamCreator: any) => {
        return streamCreator({});
      })
    };

    await TestBed.configureTestingModule({
      imports: [ReactiveFormsModule, MockAbpLocalizationPipe],
      providers: [
        { provide: DespensaItemService, useValue: mockDespensaService },
        { provide: ProductoService, useValue: mockProductoService },
        { provide: ConfirmationService, useValue: mockConfirmationService }
      ],
      schemas: [NO_ERRORS_SCHEMA] // LÍNEA CRÍTICA: ignora <abp-modal> y otras etiquetas de ABP
    })
      .overrideComponent(MyPantryComponent, {
        set: {
          imports: [CommonModule, ReactiveFormsModule, MockAbpLocalizationPipe],
          providers: [{ provide: ListService, useValue: mockListService }],
          schemas: [NO_ERRORS_SCHEMA] // También lo aplicamos al componente
        }
      })
      .compileComponents();

    fixture = TestBed.createComponent(MyPantryComponent);
    component = fixture.componentInstance;
  });

  it('debería crearse el componente', () => {
    expect(component).toBeTruthy();
  });

  it('debería mostrar estado de Error si falla la carga', () => {
    mockDespensaService.getList.and.returnValue(throwError(() => new Error('Error de red')));
    component.ngOnInit();
    fixture.detectChanges();

    expect(component.hasError).toBe(true);
    expect(component.isLoading).toBe(false);
  });

  it('debería mostrar lista vacía cuando no hay ítems', () => {
    mockDespensaService.getList.and.returnValue(of({ items: [], totalCount: 0 }));
    component.ngOnInit();
    fixture.detectChanges();

    expect(component.items.totalCount).toBe(0);
    expect(component.isLoading).toBe(false);
  });

  it('debería mostrar resultados cuando hay ítems', () => {
    const mockData = {
      items: [{ id: '1', nombreProducto: 'Arroz', estado: EstadoItemDespensa.Disponible }],
      totalCount: 1
    };
    mockDespensaService.getList.and.returnValue(of(mockData));
    component.ngOnInit();
    fixture.detectChanges();

    expect(component.items.totalCount).toBe(1);
    expect(component.items.items![0].nombreProducto).toBe('Arroz');
  });

  it('no debería llamar al servicio si la edición/creación tiene datos inválidos', () => {
    component.createItem();
    component.form.patchValue({ cantidad: -5 });
    component.save();

    expect(mockDespensaService.create).not.toHaveBeenCalled();
  });

  it('debería cancelar la eliminación sin llamar al servicio si el usuario cancela la confirmación', () => {
    mockConfirmationService.warn.and.returnValue(of(Confirmation.Status.reject));
    component.deleteItem('1', 'Arroz');
    expect(mockDespensaService.delete).not.toHaveBeenCalled();
  });

  it('debería eliminar el ítem si el usuario confirma', () => {
    mockConfirmationService.warn.and.returnValue(of(Confirmation.Status.confirm));
    component.deleteItem('1', 'Arroz');
    expect(mockDespensaService.delete).toHaveBeenCalledWith('1');
    expect(mockListService.get).toHaveBeenCalled();
  });
});
