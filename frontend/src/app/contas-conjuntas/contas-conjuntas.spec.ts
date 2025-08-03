import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ContasConjuntas } from './contas-conjuntas';

describe('ContasConjuntas', () => {
  let component: ContasConjuntas;
  let fixture: ComponentFixture<ContasConjuntas>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ContasConjuntas]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ContasConjuntas);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
