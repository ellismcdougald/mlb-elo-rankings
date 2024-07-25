import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class RatingsDataService {
  constructor(private http: HttpClient) {}
  url = 'https://td5fl4uqavlca6xy5fi67ndfuy0epobl.lambda-url.us-west-2.on.aws/';
  getRatingsData(): Observable<any> {
    return this.http.get(this.url, { headers: { Accept: 'application/json' } });
  }
}
