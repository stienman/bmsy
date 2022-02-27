import { Component, OnInit } from '@angular/core';
import { BmsyapiService } from '../bmsyapi.service';
import { interval } from 'rxjs';
import { InverterInfo } from '../inverterinfo';
import { BMSInfo } from '../bmsinfo';



@Component({
  selector: 'app-totals',
  templateUrl: './totals.component.html',
  styleUrls: ['./totals.component.css']
})

export class TotalsComponent implements OnInit {


  batteryLoadInWatts: number = 0;
  roundVals: boolean = true;
  batterySOC: string[] = [];
  pvPowerInWatts: number = 0;
  loadInWatts: number = 0;
  gridLoadInWatts: number = 0;
  batteryACChargeInWatts = 0;
  totalBattery: string = "";

  constructor(private bmsyapiService: BmsyapiService) { }

  ngOnInit(): void {
    this.getData();
    interval(5000).subscribe(() => { this.getData(); });
  }

  getData(): void {
    this.getTotals();
  }

  getOptiLabel(val: number): string {
    if (this.roundVals) {
      if (val > 1000) {
        return `${(val / 1000).toFixed(1)} kWh`;
      } else return `${val} Wh`;
    } else return `${val} Wh`;
  }

  getTotals(): void {

    this.bmsyapiService.getInverterInfo().subscribe(resultaat => {

      this.pvPowerInWatts = 0;
      this.loadInWatts = 0;
      this.gridLoadInWatts = 0;
      this.batteryACChargeInWatts = 0;
      this.batteryLoadInWatts = 0;

      resultaat.forEach(element => {
        this.pvPowerInWatts += element.pvPowerInWatts;
        this.loadInWatts += element.loadInWatts;
        this.gridLoadInWatts += element.gridLoadInWatts;
        this.batteryACChargeInWatts += element.batteryACChargeInWatts;
        this.batteryLoadInWatts += element.batteryLoadInWatts;
      });
    });

    this.batteryLoadInWatts = this.batteryLoadInWatts / -1;
    this.totalBattery = `${this.batteryLoadInWatts > 0 ? "+" : ""}${this.batteryLoadInWatts}`;


    let tmp: string[] =[];
    

    this.bmsyapiService.getBMSInfo().subscribe(soc => {
      if (soc.length > 0) {
        soc.forEach(bms => {
          tmp.push(bms.soc.toString());
        });
      }
      this.batterySOC = tmp;
    });
  }
}
