import { TestBed } from '@angular/core/testing';

import { RatingsDataService } from './ratings-data.service';

describe('RatingsDataService', () => {
  let service: RatingsDataService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(RatingsDataService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
