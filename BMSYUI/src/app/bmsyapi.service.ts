import { Injectable } from '@angular/core';
import { InverterInfo } from './inverterinfo';
import { Observable, observable, of } from 'rxjs';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { catchError, map, tap } from 'rxjs/operators';

import { BMSInfo } from './bmsinfo';

@Injectable({
  providedIn: 'root'
})
export class BmsyapiService {
  private endpointUrl = "http://10.0.0.18:2407";
  constructor(private http: HttpClient) { }

  getInverterInfo(): Observable<InverterInfo[]> {
    // return this.http.get<Growatt[]>(`${this.endpointUrl}/GetGrowattStatuses`).pipe(tap(_ => this.log('Fetched growatts')),
    //     catchError(this.handleError<Growatt[]>('GetGrowattStatuses', []))
    //   );
      return this.http.get<InverterInfo[]>(`${this.endpointUrl}/GetInverterStatuses`);
  }

  getBMSInfo(): Observable<BMSInfo[]> {
    // return this.http.get<Growatt[]>(`${this.endpointUrl}/GetGrowattStatuses`).pipe(tap(_ => this.log('Fetched growatts')),
    //     catchError(this.handleError<Growatt[]>('GetGrowattStatuses', []))
    //   );
      return this.http.get<BMSInfo[]>(`${this.endpointUrl}/GetBMSStatuses`);
  }

  setFloatVoltage(voltage: string) {
    this.http.get(`${this.endpointUrl}/SetFloatVoltage/${voltage}`).subscribe(data => { console.log(data); });
  }

  setBulkVoltage(voltage: string) {
    this.http.get(`${this.endpointUrl}/SetBulkVoltage/${voltage}`).subscribe(data => { console.log(data); });
  }

  setBatteryLowBackToGrid(voltage: string) {
    this.http.get(`${this.endpointUrl}/SetBatteryLowBackToGrid/${voltage}`).subscribe(data => { console.log(data); });
  }

  setBackToBatt(voltage: string) {
    this.http.get(`${this.endpointUrl}/SetBackToBattery/${voltage}`).subscribe(data => { console.log(data); });
  }

  setBattCuttOff(voltage: string) {
    this.http.get(`${this.endpointUrl}/SetBatteryCutOffVoltage/${voltage}`).subscribe(data => { console.log(data); });
  }
  
  setOutputSource(outputsource: number) {
    this.http.get(`${this.endpointUrl}/SetOutputSource/${outputsource}`).subscribe(data => { console.log(data); });
  }

  setChargingSource(chargingSource: number) {
    this.http.get(`${this.endpointUrl}/SetChargingSource/${chargingSource}`).subscribe(data => { console.log(data); });
  }

  setChargingCurrent(chargingCurrent: number) {
    this.http.get(`${this.endpointUrl}/SetChargingCurrentAC/${chargingCurrent}`).subscribe(data => { console.log(data); });
  }

  private log(message: string) {
    console.log(message)
  }

  private handleError<T>(operation = 'operation', result?: T) {
    return (error: any): Observable<T> => {
      console.error(error); // log to console instead
      this.log(`${operation} failed: ${error.message}`);
      return of(result as T);
    };
  }
}
