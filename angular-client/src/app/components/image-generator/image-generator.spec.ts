import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ImageGenerator } from './image-generator';

describe('ImageGenerator', () => {
  let component: ImageGenerator;
  let fixture: ComponentFixture<ImageGenerator>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ImageGenerator]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ImageGenerator);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
