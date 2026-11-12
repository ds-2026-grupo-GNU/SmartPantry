
import { ComponentFixture, TestBed, getTestBed } from '@angular/core/testing';

//import { NO_ERRORS_SCHEMA } from '@angular/core';
import {
  NO_ERRORS_SCHEMA,
  Pipe,
  PipeTransform,
} from '@angular/core';

import {
  BrowserDynamicTestingModule,
  platformBrowserDynamicTesting,
} from '@angular/platform-browser-dynamic/testing';

import { of, throwError } from 'rxjs';

import { MyPantryComponent } from './my-pantry.component';

import { DespensaItemService } from '../proxy/despensa/despensa-item.service';
import { ProductoService } from '../proxy/productos/producto.service';

//import { ConfirmationService, Confirmation } from '@abp/ng.theme.shared';
//import { ListService } from '@abp/ng.core';

import { CoreModule, ListService } from '@abp/ng.core';
import {
  ThemeSharedModule,
  ConfirmationService,
  Confirmation,
} from '@abp/ng.theme.shared';

import { EstadoItemDespensa } from '../proxy/despensa/estado-item-despensa.enum';

@Pipe({
  name: 'abpLocalization',
  standalone: true,
})
class MockAbpLocalizationPipe implements PipeTransform {
  transform(value: string): string {
    return value;
  }
}

try {
  getTestBed().initTestEnvironment(
    BrowserDynamicTestingModule,
    platformBrowserDynamicTesting()
  );
} catch {
  // El entorno ya fue inicializado.
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
      getList: jasmine.createSpy('getList').and.returnValue(
        of({ items: [], totalCount: 0 })
      ),
      create: jasmine.createSpy('create').and.returnValue(of({})),
      update: jasmine.createSpy('update').and.returnValue(of({})),
      marcarComoConsumido: jasmine.createSpy('marcarComoConsumido').and.returnValue(of({})),
      delete: jasmine.createSpy('delete').and.returnValue(of({})),
    };

    mockProductoService = {
      getList: jasmine.createSpy('getList').and.returnValue(
        of({ items: [], totalCount: 0 })
      ),
    };

    mockConfirmationService = {
      warn: jasmine.createSpy('warn').and.returnValue(
        of(Confirmation.Status.confirm)
      ),
    };

    mockListService = {
      get: jasmine.createSpy('get'),
      hookToQuery: jasmine.createSpy('hookToQuery').and.callFake(
        (streamCreator: any) => streamCreator({})
      ),
    };

    await TestBed.configureTestingModule({
      imports: [MyPantryComponent],
      schemas: [NO_ERRORS_SCHEMA],
    })
      .overrideComponent(MyPantryComponent, {
  remove: {
    providers: [ListService],
    imports: [CoreModule, ThemeSharedModule],
  },
  add: {
    imports: [MockAbpLocalizationPipe],
    schemas: [NO_ERRORS_SCHEMA],
    providers: [
      { provide: ListService, useValue: mockListService },
      { provide: DespensaItemService, useValue: mockDespensaService },
      { provide: ProductoService, useValue: mockProductoService },
      {
        provide: ConfirmationService,
        useValue: mockConfirmationService,
      },
    ],
  },
})
      .compileComponents();

    fixture = TestBed.createComponent(MyPantryComponent);
    component = fixture.componentInstance;
  });

  it('deberia crearse el componente', () => {
    fixture.detectChanges();

    expect(component).toBeTruthy();
  });

  it('deberia mostrar estado de error si falla la carga', () => {
    mockDespensaService.getList.and.returnValue(
      throwError(() => new Error('Error de red'))
    );

    fixture.detectChanges();

    expect(component.hasError).toBe(true);
    expect(component.isLoading).toBe(false);
  });

  it('deberia mostrar lista vacia cuando no hay items', () => {
    mockDespensaService.getList.and.returnValue(
      of({ items: [], totalCount: 0 })
    );

    fixture.detectChanges();

    expect(component.items.totalCount).toBe(0);
    expect(component.isLoading).toBe(false);
  });

  it('deberia mostrar resultados cuando hay items', () => {
    const mockData = {
      items: [
        {
          id: '1',
          nombreProducto: 'Arroz',
          estado: EstadoItemDespensa.Disponible,
        },
      ],
      totalCount: 1,
    };

    mockDespensaService.getList.and.returnValue(of(mockData));

    fixture.detectChanges();

    expect(component.items.totalCount).toBe(1);
    expect(component.items.items![0].nombreProducto).toBe('Arroz');
  });

  it('no deberia llamar al servicio si la creacion tiene datos invalidos', () => {
    component.createItem();
    component.form.patchValue({ cantidad: -5 });

    component.save();

    expect(mockDespensaService.create).not.toHaveBeenCalled();
  });

  it('deberia cancelar la eliminacion sin llamar al servicio si el usuario cancela', () => {
    mockConfirmationService.warn.and.returnValue(
      of(Confirmation.Status.reject)
    );

    component.deleteItem('1', 'Arroz');

    expect(mockDespensaService.delete).not.toHaveBeenCalled();
  });

  it('deberia eliminar el item si el usuario confirma', () => {
    mockConfirmationService.warn.and.returnValue(
      of(Confirmation.Status.confirm)
    );

    component.deleteItem('1', 'Arroz');

    expect(mockDespensaService.delete).toHaveBeenCalledWith('1');
    expect(mockListService.get).toHaveBeenCalled();
  });
});
