import { ComponentFixture, TestBed } from '@angular/core/testing';

import { EditFlightDialogComponent } from './edit-flight-dialog.component';

describe('EditFlightDialogComponent', () => {
  let component: EditFlightDialogComponent;
  let fixture: ComponentFixture<EditFlightDialogComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ EditFlightDialogComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(EditFlightDialogComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
