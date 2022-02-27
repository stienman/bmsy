import { ComponentFixture, TestBed } from '@angular/core/testing';

import { BMSYSettingsComponent } from './bmsysettings.component';

describe('BMSYSettingsComponent', () => {
  let component: BMSYSettingsComponent;
  let fixture: ComponentFixture<BMSYSettingsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ BMSYSettingsComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(BMSYSettingsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
