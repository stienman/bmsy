import { TestBed } from '@angular/core/testing';

import { BmsyapiService } from './bmsyapi.service';

describe('BmsyapiService', () => {
  let service: BmsyapiService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(BmsyapiService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
